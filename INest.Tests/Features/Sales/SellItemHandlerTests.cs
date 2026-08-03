using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using INest.Constants;
using INest.Data.Entities.Core;
using INest.Data.Entities.Finances;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Exceptions;
using INest.Features.Sales.Commands.CancelSale;
using INest.Features.Sales.Commands.DeleteSaleRecord;
using INest.Features.Sales.Commands.SellItem;
using INest.Features.Sales.DTOs;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Tracker;
using INest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace INest.Tests.Features.Sales
{
    public class SalesHandlersTests
    {
        private readonly ISanitizerService _sanitizerMock = Substitute.For<ISanitizerService>();
        private readonly ICacheTracker _trackerMock = Substitute.For<ICacheTracker>();

        public SalesHandlersTests()
        {
            _sanitizerMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.Arg<string>());
        }

        #region SellItemHandler Tests

        [Fact]
        public async Task SellItem_ShouldSuccessfullySell_RemoveFromLocation_ClearReminders_AndCalculateProfit()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var locationId = Guid.NewGuid();

            // ДОБАВИЛИ Color = "#00B894"
            var category = new Category { Id = Guid.NewGuid(), Name = "Электроника", Color = "#00B894", UserId = userId };
            var item = new Item
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Макбук",
                CategoryId = category.Id,
                Category = category,
                StorageLocationId = locationId,
                Details = new ItemDetails { PurchasePrice = 1000m, Currency = "USD" }
            };

            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = userId,
                Title = "Почистить клавиатуру"
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Reminders.Add(reminder);
            await db.SaveChangesAsync();

            var dto = new SellItemRequestDto
            {
                ItemId = item.Id,
                SalePrice = 1500m,
                PlatformFee = 50m,
                SoldDate = DateTime.UtcNow,
                Comment = "Продал на ОЛХ"
            };

            var handler = new SellItemHandler(db, _sanitizerMock, _trackerMock);
            var command = new SellItemCommand(userId, dto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.SalePrice.ShouldBe(1500m);
            result.Profit.ShouldBe(500m);

            var itemInDb = await db.Items.FirstAsync(i => i.Id == item.Id);
            itemInDb.Status.ShouldBe(ItemStatus.Sold);
            itemInDb.StorageLocationId.ShouldBeNull();

            var remindersInDb = await db.Reminders.Where(r => r.ItemId == item.Id).ToListAsync();
            remindersInDb.ShouldBeEmpty();

            var saleInDb = await db.Sales.FirstOrDefaultAsync(s => s.ItemId == item.Id);
            saleInDb.ShouldNotBeNull();
            saleInDb.Profit.ShouldBe(500m);

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task SellItem_ShouldThrowAppException_WhenItemIsAlreadySoldOrArchived()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var item = new Item
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Уже проданный перфоратор",
                CategoryId = category.Id,
                Category = category
            };
            item.Sell(); // Устанавливаем статус Sold

            db.Categories.Add(category);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var dto = new SellItemRequestDto { ItemId = item.Id, SalePrice = 500m };
            var handler = new SellItemHandler(db, _sanitizerMock, _trackerMock);
            var command = new SellItemCommand(userId, dto);

            // Act & Assert
            var ex = await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(command, CancellationToken.None);
            });

            ex.Message.ShouldBe(LocalizationConstants.ITEMS.ERRORS.CANNOT_SELL);
        }

        [Fact]
        public async Task SellItem_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var handler = new SellItemHandler(db, _sanitizerMock, _trackerMock);
            var dto = new SellItemRequestDto { ItemId = Guid.NewGuid(), SalePrice = 100m };

            // Act & Assert
            await Should.ThrowAsync<KeyNotFoundException>(async () =>
            {
                await handler.Handle(new SellItemCommand(Guid.NewGuid(), dto), CancellationToken.None);
            });
        }

        #endregion

        #region CancelSaleHandler Tests

        [Fact]
        public async Task CancelSale_ShouldReturnItemToActiveStatus_SetLocation_AndAddHistory()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();
            var newLocationId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Одежда", Color = "#00B894", UserId = userId };
            var item = new Item
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Куртка",
                CategoryId = category.Id,
                Category = category
            };
            item.Sell();

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                ItemNameSnapshot = item.Name,
                SalePrice = 200m
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Sales.Add(sale);
            await db.SaveChangesAsync();

            var handler = new CancelSaleHandler(db, _trackerMock);
            var command = new CancelSaleCommand(userId, item.Id, newLocationId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            var saleInDb = await db.Sales.FirstOrDefaultAsync(s => s.Id == sale.Id);
            saleInDb.ShouldBeNull();

            var itemInDb = await db.Items.FirstAsync(i => i.Id == item.Id);
            itemInDb.Status.ShouldBe(ItemStatus.Active);
            itemInDb.StorageLocationId.ShouldBe(newLocationId);
        }

        #endregion

        #region DeleteSaleRecordHandler Tests

        [Fact]
        public async Task DeleteSaleRecord_ShouldCascadeDeleteAssociatedItem_IfItemExists()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Разное", Color = "#00B894", UserId = userId };
            var item = new Item
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Вещь под удаление",
                CategoryId = category.Id,
                Category = category
            };

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                ItemNameSnapshot = item.Name,
                SalePrice = 300m
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Sales.Add(sale);
            await db.SaveChangesAsync();

            var handler = new DeleteSaleRecordHandler(db, _trackerMock);
            var command = new DeleteSaleRecordCommand(userId, sale.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            (await db.Sales.FirstOrDefaultAsync(s => s.Id == sale.Id)).ShouldBeNull();
            (await db.Items.FirstOrDefaultAsync(i => i.Id == item.Id)).ShouldBeNull();

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        #endregion
    }
}