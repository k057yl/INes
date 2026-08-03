using INest.Constants;
using INest.Data.Entities.Core;
using INest.Exceptions;
using INest.Features.Categories.Commands.CreateCategory;
using INest.Features.Categories.Commands.DeleteCategory;
using INest.Features.Categories.Commands.UpdateCategory;
using INest.Features.Categories.DTOs;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Tracker;
using INest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Categories
{
    public class CategoriesHandlersTests
    {
        private readonly ISanitizerService _sanitizerMock = Substitute.For<ISanitizerService>();
        private readonly ICacheTracker _trackerMock = Substitute.For<ICacheTracker>();

        public CategoriesHandlersTests()
        {
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns(x => x.Arg<string>()?.Trim());
        }

        #region CreateCategoryHandler Tests

        [Fact]
        public async Task CreateCategory_ShouldCreateCategory_WithDefaultColor()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var dto = new CreateCategoryDto { Name = "Инструменты", Color = null };
            var handler = new CreateCategoryHandler(db, _sanitizerMock, _trackerMock);
            var command = new CreateCategoryCommand(userId, dto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Инструменты");
            result.Color.ShouldBe("#007bff");

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task CreateCategory_ShouldThrowAppException_WhenNameIsInvalid()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns("");

            var dto = new CreateCategoryDto { Name = "   " };
            var handler = new CreateCategoryHandler(db, _sanitizerMock, _trackerMock);

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(new CreateCategoryCommand(Guid.NewGuid(), dto), CancellationToken.None);
            });
        }

        #endregion

        #region DeleteCategoryHandler Tests

        [Fact]
        public async Task DeleteCategory_ShouldThrowInvalidOperationException_WhenDeletingDefaultCategory()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var defaultCat = new Category
            {
                Id = Guid.NewGuid(),
                Name = SharedConstants.CATEGORY_OTHER,
                Color = "#64748b",
                UserId = userId
            };

            db.Categories.Add(defaultCat);
            await db.SaveChangesAsync();

            var handler = new DeleteCategoryHandler(db, _trackerMock);
            var command = new DeleteCategoryCommand(userId, defaultCat.Id, null);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        [Fact]
        public async Task DeleteCategory_ShouldReassignItemsToDefaultCategory_WhenNoTargetCategoryProvided()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var catToDelete = new Category { Id = Guid.NewGuid(), Name = "Строительство", Color = "#00B894", UserId = userId };
            var item1 = new Item { Id = Guid.NewGuid(), Name = "Дрель", UserId = userId, CategoryId = catToDelete.Id };
            var item2 = new Item { Id = Guid.NewGuid(), Name = "Молоток", UserId = userId, CategoryId = catToDelete.Id };

            db.Categories.Add(catToDelete);
            db.Items.AddRange(item1, item2);
            await db.SaveChangesAsync();

            var handler = new DeleteCategoryHandler(db, _trackerMock);
            var command = new DeleteCategoryCommand(userId, catToDelete.Id, null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            // 1. Категория удалена
            (await db.Categories.FirstOrDefaultAsync(c => c.Id == catToDelete.Id)).ShouldBeNull();

            // 2. Автоматически создалась категория SharedConstants.CATEGORY_OTHER
            var defaultCat = await db.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == SharedConstants.CATEGORY_OTHER);
            defaultCat.ShouldNotBeNull();

            // 3. Предметы перепривязаны к defaultCat
            var itemsInDb = await db.Items.Where(i => i.UserId == userId).ToListAsync();
            itemsInDb.All(i => i.CategoryId == defaultCat.Id).ShouldBeTrue();

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task DeleteCategory_ShouldReassignItemsToSpecifiedTargetCategory()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var catToDelete = new Category { Id = Guid.NewGuid(), Name = "Старая категория", Color = "#00B894", UserId = userId };
            var targetCat = new Category { Id = Guid.NewGuid(), Name = "Новая категория", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Шурупы", UserId = userId, CategoryId = catToDelete.Id };

            db.Categories.AddRange(catToDelete, targetCat);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var handler = new DeleteCategoryHandler(db, _trackerMock);
            var command = new DeleteCategoryCommand(userId, catToDelete.Id, targetCat.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            var itemInDb = await db.Items.FirstAsync(i => i.Id == item.Id);
            itemInDb.CategoryId.ShouldBe(targetCat.Id);
        }

        #endregion

        #region UpdateCategoryHandler Tests

        [Fact]
        public async Task UpdateCategory_ShouldThrowAppException_WhenSelfNestingParentCategory()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var category = new Category { Id = Guid.NewGuid(), Name = "Посуда", Color = "#00B894", UserId = userId };

            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var dto = new CreateCategoryDto { Name = "Посуда", Color = "#00B894", ParentCategoryId = category.Id };
            var handler = new UpdateCategoryHandler(db, _sanitizerMock, _trackerMock);
            var command = new UpdateCategoryCommand(userId, category.Id, dto);

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        #endregion
    }
}