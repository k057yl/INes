using INest.Constants;
using INest.Data.Entities.Core;
using INest.Exceptions;
using INest.Features.Locations.Commands.CreateLocation;
using INest.Features.Locations.Commands.DeleteLocation;
using INest.Features.Locations.Commands.MoveLocation;
using INest.Features.Locations.DTOs;
using INest.Features.Locations.Queries.GetLocationTree;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Tracker;
using INest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Locations
{
    public class LocationsHandlersTests
    {
        private readonly ISanitizerService _sanitizerMock = Substitute.For<ISanitizerService>();
        private readonly ICacheTracker _trackerMock = Substitute.For<ICacheTracker>();

        public LocationsHandlersTests()
        {
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns(x => x.Arg<string>()?.Trim());
            _sanitizerMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.Arg<string>());
        }

        #region CreateLocationHandler Tests

        [Fact]
        public async Task CreateLocation_ShouldCreateSuccessfully_WithDefaultColorAndIcon()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var dto = new CreateLocationDto
            {
                Name = "Кладовка",
                Description = "Для коробок",
                ParentLocationId = null,
                SortOrder = 0
            };

            var handler = new CreateLocationHandler(db, _sanitizerMock, _trackerMock);
            var command = new CreateLocationCommand(userId, dto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Кладовка");
            result.Color.ShouldBe("#007bff");
            result.Icon.ShouldBe("fa-folder");

            var inDb = await db.StorageLocations.FirstOrDefaultAsync(l => l.Id == result.Id);
            inDb.ShouldNotBeNull();

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task CreateLocation_ShouldThrowAppException_WhenParentDoesNotExist()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var dto = new CreateLocationDto { Name = "Подпапка", ParentLocationId = Guid.NewGuid() };
            var handler = new CreateLocationHandler(db, _sanitizerMock, _trackerMock);

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(new CreateLocationCommand(Guid.NewGuid(), dto), CancellationToken.None);
            });
        }

        #endregion

        #region MoveLocationHandler Tests

        [Fact]
        public async Task MoveLocation_ShouldThrowInvalidOperationException_WhenNestingIntoItself()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var loc = new StorageLocation { Id = Guid.NewGuid(), Name = "Гараж", UserId = userId };

            db.StorageLocations.Add(loc);
            await db.SaveChangesAsync();

            var handler = new MoveLocationHandler(db, _trackerMock);
            var command = new MoveLocationCommand(userId, loc.Id, loc.Id); // NewParentId == LocationId

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        [Fact]
        public async Task MoveLocation_ShouldThrowInvalidOperationException_WhenCircularDependencyDetected()
        {
            // Arrange
            // Иерархия из 2 уровней: Root (1) -> Child (2), чтобы не выбить лимит глубины (3)
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var root = new StorageLocation { Id = Guid.NewGuid(), Name = "Root", UserId = userId };
            var child = new StorageLocation { Id = Guid.NewGuid(), Name = "Child", UserId = userId, ParentLocationId = root.Id };

            db.StorageLocations.AddRange(root, child);
            await db.SaveChangesAsync();

            var handler = new MoveLocationHandler(db, _trackerMock);

            // Попытка переместить родителя Root внутрь своего ребенка Child
            var command = new MoveLocationCommand(userId, root.Id, child.Id);

            // Act & Assert
            var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });

            ex.Message.ShouldBe(LocalizationConstants.LOCATIONS.ERRORS.CIRCULAR_DEPENDENCY);
        }

        [Fact]
        public async Task MoveLocation_ShouldThrowAppException_WhenMaxDepthExceeded()
        {
            // Arrange
            // Иерархия глубина 3: L1 -> L2 -> L3
            // Пытаемся засунуть L4 внутрь L3 (суммарная глубина станет 4 > 3)
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var l1 = new StorageLocation { Id = Guid.NewGuid(), Name = "L1", UserId = userId };
            var l2 = new StorageLocation { Id = Guid.NewGuid(), Name = "L2", UserId = userId, ParentLocationId = l1.Id };
            var l3 = new StorageLocation { Id = Guid.NewGuid(), Name = "L3", UserId = userId, ParentLocationId = l2.Id };
            var l4 = new StorageLocation { Id = Guid.NewGuid(), Name = "L4", UserId = userId };

            db.StorageLocations.AddRange(l1, l2, l3, l4);
            await db.SaveChangesAsync();

            var handler = new MoveLocationHandler(db, _trackerMock);
            var command = new MoveLocationCommand(userId, l4.Id, l3.Id);

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        [Fact]
        public async Task MoveLocation_ShouldSuccessfullyMove_AndRecalculateSortOrder()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var root1 = new StorageLocation { Id = Guid.NewGuid(), Name = "Root 1", UserId = userId };
            var root2 = new StorageLocation { Id = Guid.NewGuid(), Name = "Root 2", UserId = userId };
            var existingChildInRoot2 = new StorageLocation { Id = Guid.NewGuid(), Name = "Child 1", UserId = userId, ParentLocationId = root2.Id, SortOrder = 5 };

            db.StorageLocations.AddRange(root1, root2, existingChildInRoot2);
            await db.SaveChangesAsync();

            var handler = new MoveLocationHandler(db, _trackerMock);
            var command = new MoveLocationCommand(userId, root1.Id, root2.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            var movedLoc = await db.StorageLocations.FirstAsync(l => l.Id == root1.Id);
            movedLoc.ParentLocationId.ShouldBe(root2.Id);
            movedLoc.SortOrder.ShouldBe(6); // 5 + 1
        }

        #endregion

        #region DeleteLocationHandler Tests

        [Fact]
        public async Task DeleteLocation_ShouldMoveItemsToOther_AndRemoveLocation_WhenTargetNotSpecified()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };

            var location = new StorageLocation { Id = Guid.NewGuid(), Name = "Непустая", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Вещь", UserId = userId, CategoryId = category.Id, StorageLocationId = location.Id };

            db.Categories.Add(category);
            db.StorageLocations.Add(location);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var handler = new DeleteLocationHandler(db, _trackerMock);

            // ФИКС: Сначала location.Id, затем userId!
            var command = new DeleteLocationCommand(location.Id, userId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            var deletedLoc = await db.StorageLocations.FirstOrDefaultAsync(l => l.Id == location.Id);
            deletedLoc.ShouldBeNull();

            var otherLoc = await db.StorageLocations.FirstOrDefaultAsync(l => l.UserId == userId && l.Name == "Other");
            otherLoc.ShouldNotBeNull();

            var movedItem = await db.Items.FirstOrDefaultAsync(i => i.Id == item.Id);
            movedItem.ShouldNotBeNull();
            movedItem.StorageLocationId.ShouldBe(otherLoc.Id);

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task DeleteLocation_ShouldMoveItemsToSpecifiedTarget_AndRemoveLocation()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };

            var location = new StorageLocation { Id = Guid.NewGuid(), Name = "Удаляемая", UserId = userId };
            var targetLocation = new StorageLocation { Id = Guid.NewGuid(), Name = "Целевая", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Вещь", UserId = userId, CategoryId = category.Id, StorageLocationId = location.Id };

            db.Categories.Add(category);
            db.StorageLocations.AddRange(location, targetLocation);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var handler = new DeleteLocationHandler(db, _trackerMock);

            // ФИКС: Сначала location.Id, затем userId, затем targetLocation.Id!
            var command = new DeleteLocationCommand(location.Id, userId, targetLocation.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            var deletedLoc = await db.StorageLocations.FirstOrDefaultAsync(l => l.Id == location.Id);
            deletedLoc.ShouldBeNull();

            var movedItem = await db.Items.FirstOrDefaultAsync(i => i.Id == item.Id);
            movedItem.ShouldNotBeNull();
            movedItem.StorageLocationId.ShouldBe(targetLocation.Id);
        }

        [Fact]
        public async Task DeleteLocation_ShouldThrowAppException_WhenNestingIntoItself()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var location = new StorageLocation { Id = Guid.NewGuid(), Name = "Удаляемая", UserId = userId };
            var childLoc = new StorageLocation { Id = Guid.NewGuid(), Name = "Дочерняя", UserId = userId, ParentLocationId = location.Id };

            db.StorageLocations.AddRange(location, childLoc);
            await db.SaveChangesAsync();

            var handler = new DeleteLocationHandler(db, _trackerMock);

            // ФИКС: Сначала location.Id, затем userId, затем target (location.Id)
            var command = new DeleteLocationCommand(location.Id, userId, location.Id);

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        #endregion

        #region GetLocationTreeHandler Tests

        [Fact]
        public async Task GetLocationTree_ShouldBuildHierarchicalTreeAndFilterSoldItems()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };

            var root = new StorageLocation { Id = Guid.NewGuid(), Name = "Шкаф", UserId = userId, SortOrder = 0 };
            var child = new StorageLocation { Id = Guid.NewGuid(), Name = "Полка 1", UserId = userId, ParentLocationId = root.Id, SortOrder = 0 };

            var activeItem = new Item { Id = Guid.NewGuid(), Name = "Носки", UserId = userId, CategoryId = category.Id, StorageLocationId = child.Id };
            var soldItem = new Item { Id = Guid.NewGuid(), Name = "Старый свитер", UserId = userId, CategoryId = category.Id, StorageLocationId = child.Id };
            soldItem.Sell();

            db.Categories.Add(category);
            db.StorageLocations.AddRange(root, child);
            db.Items.AddRange(activeItem, soldItem);
            await db.SaveChangesAsync();

            var handler = new GetLocationTreeHandler(db);
            var query = new GetLocationTreeQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Count.ShouldBe(1); // Должна быть только 1 корневая локация
            var rootNode = result[0];
            rootNode.Id.ShouldBe(root.Id);

            rootNode.Children.Count.ShouldBe(1);
            var childNode = rootNode.Children[0];
            childNode.Id.ShouldBe(child.Id);

            // Проданный свитер должен отфильтроваться
            childNode.Items.Count.ShouldBe(1);
            childNode.Items[0].Name.ShouldBe("Носки");
        }

        #endregion
    }
}