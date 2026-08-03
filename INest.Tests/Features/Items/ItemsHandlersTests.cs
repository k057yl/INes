using INest.Data.Entities.Core;
using INest.Data.Enums;
using INest.Exceptions;
using INest.Features.Items.Commands.ChangeItemStatus;
using INest.Features.Items.Commands.CreateItem;
using INest.Features.Items.Commands.DeleteArchivedItem;
using INest.Features.Items.Commands.MoveItem;
using INest.Features.Items.DTOs;
using INest.Infrastructure.Email;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Storage;
using INest.Infrastructure.Tracker;
using INest.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Items
{
    public class ItemsHandlersTests
    {
        private readonly IPhotoService _photoServiceMock = Substitute.For<IPhotoService>();
        private readonly ISanitizerService _sanitizerMock = Substitute.For<ISanitizerService>();
        private readonly ICacheTracker _trackerMock = Substitute.For<ICacheTracker>();
        private readonly IEmailService _emailServiceMock = Substitute.For<IEmailService>();
        private readonly ILogger<CreateItemHandler> _createLoggerMock = Substitute.For<ILogger<CreateItemHandler>>();
        private readonly ILogger<LendingService> _lendingLoggerMock = Substitute.For<ILogger<LendingService>>();

        public ItemsHandlersTests()
        {
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns(x => x.Arg<string>()?.Trim());
            _sanitizerMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.Arg<string>());
        }

        #region CreateItemHandler Tests

        [Fact]
        public async Task CreateItem_ShouldCreateActiveItem_WithDetailsAndHistory()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Электроника", Color = "#00B894", UserId = userId };
            var location = new StorageLocation { Id = Guid.NewGuid(), Name = "Стол", UserId = userId };

            db.Categories.Add(category);
            db.StorageLocations.Add(location);
            await db.SaveChangesAsync();

            var dto = new CreateItemDto
            {
                Name = "Ноутбук",
                Description = "Игровой",
                CategoryId = category.Id,
                StorageLocationId = location.Id,
                Status = ItemStatus.Active,
                Details = new ItemFinanceDto
                {
                    PurchasePrice = 1500m,
                    Currency = "USD"
                }
            };

            var lendingService = new LendingService(db, _emailServiceMock, _lendingLoggerMock);
            var handler = new CreateItemHandler(db, _photoServiceMock, lendingService, _sanitizerMock, _createLoggerMock, _trackerMock);
            var command = new CreateItemCommand(userId, dto, new List<IFormFile>());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Ноутбук");
            result.Status.ShouldBe(ItemStatus.Active);
            result.Details.ShouldNotBeNull();
            result.Details.PurchasePrice.ShouldBe(1500m);

            var itemInDb = await db.Items.Include(i => i.Details).FirstOrDefaultAsync(i => i.Id == result.Id);
            itemInDb.ShouldNotBeNull();

            var history = await db.ItemHistories.FirstOrDefaultAsync(h => h.ItemId == result.Id);
            history.ShouldNotBeNull();
            history.Type.ShouldBe(ItemHistoryType.Created);

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task CreateItem_ShouldThrowAppException_WhenNameIsInvalidAfterSanitizing()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns("");

            var dto = new CreateItemDto { Name = "<script></script>" };
            var lendingService = new LendingService(db, _emailServiceMock, _lendingLoggerMock);
            var handler = new CreateItemHandler(db, _photoServiceMock, lendingService, _sanitizerMock, _createLoggerMock, _trackerMock);

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(new CreateItemCommand(Guid.NewGuid(), dto, new List<IFormFile>()), CancellationToken.None);
            });
        }

        #endregion

        #region ChangeItemStatusHandler Tests

        [Fact]
        public async Task ChangeItemStatus_ShouldUpdateStatus_AndAddHistoryRecord()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Шуруповерт", UserId = userId, CategoryId = category.Id };

            db.Categories.Add(category);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var handler = new ChangeItemStatusHandler(db, _trackerMock);
            var command = new ChangeItemStatusCommand(userId, item.Id, ItemStatus.Archived);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            var itemInDb = await db.Items.FirstAsync(i => i.Id == item.Id);
            itemInDb.Status.ShouldBe(ItemStatus.Archived);

            var history = await db.ItemHistories.FirstOrDefaultAsync(h => h.ItemId == item.Id && h.Type == ItemHistoryType.Archived);
            history.ShouldNotBeNull();
            history.NewValue.ShouldBe(ItemStatus.Archived.ToString());

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        #endregion

        #region MoveItemHandler Tests

        [Fact]
        public async Task MoveItem_ShouldUpdateLocation_AndRecordHistory()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var loc1 = new StorageLocation { Id = Guid.NewGuid(), Name = "Гараж", UserId = userId };
            var loc2 = new StorageLocation { Id = Guid.NewGuid(), Name = "Подвал", UserId = userId };

            var item = new Item { Id = Guid.NewGuid(), Name = "Набор ключей", UserId = userId, CategoryId = category.Id, StorageLocationId = loc1.Id };

            db.Categories.Add(category);
            db.StorageLocations.AddRange(loc1, loc2);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var handler = new MoveItemHandler(db, _trackerMock);
            var command = new MoveItemCommand(userId, item.Id, loc2.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            var itemInDb = await db.Items.FirstAsync(i => i.Id == item.Id);
            itemInDb.StorageLocationId.ShouldBe(loc2.Id);

            var history = await db.ItemHistories.FirstOrDefaultAsync(h => h.ItemId == item.Id && h.Type == ItemHistoryType.Moved);
            history.ShouldNotBeNull();
            history.OldValue.ShouldBe("Гараж");
            history.NewValue.ShouldBe("Подвал");

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        #endregion

        #region DeleteArchivedItemHandler Tests

        [Fact]
        public async Task DeleteArchivedItem_ShouldHardDelete_AndRemovePhotosFromCloud()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Старая тумба", UserId = userId, CategoryId = category.Id };
            item.Archive();

            var photo = new ItemPhoto
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                PublicId = "cloudinary_photo_123",
                FilePath = "https://cloud.com/photo.jpg"
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.ItemPhotos.Add(photo);
            await db.SaveChangesAsync();

            var handler = new DeleteArchivedItemHandler(db, _photoServiceMock, _trackerMock);
            var command = new DeleteArchivedItemCommand(userId, item.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            (await db.Items.FirstOrDefaultAsync(i => i.Id == item.Id)).ShouldBeNull();
            await _photoServiceMock.Received(1).DeletePhotoAsync("cloudinary_photo_123");
            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task DeleteArchivedItem_ShouldThrowAppException_WhenItemIsNotArchived()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Активный телефон", UserId = userId, CategoryId = category.Id };

            db.Categories.Add(category);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var handler = new DeleteArchivedItemHandler(db, _photoServiceMock, _trackerMock);
            var command = new DeleteArchivedItemCommand(userId, item.Id);

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });
        }

        #endregion
    }
}