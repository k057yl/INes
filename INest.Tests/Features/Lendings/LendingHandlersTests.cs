using System;
using System.Collections.Generic;
using INest.Data.Entities.Core;
using INest.Data.Entities.Finances;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Features.Lendings.Commands.LendItem;
using INest.Features.Lendings.Commands.ReturnItem;
using INest.Features.Lendings.DTOs;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Tracker;
using INest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Lendings
{
    public class LendingHandlersTests
    {
        private readonly ISanitizerService _sanitizerMock = Substitute.For<ISanitizerService>();
        private readonly ICacheTracker _trackerMock = Substitute.For<ICacheTracker>();

        public LendingHandlersTests()
        {
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns(x => x.Arg<string>()?.Trim());
            _sanitizerMock.SanitizeHtml(Arg.Any<string>()).Returns(x => x.Arg<string>());
        }

        #region LendItemHandler Tests

        [Fact]
        public async Task LendItem_ShouldLendOwnItem_ChangeStatusToLent_AndCreateReminderIfExpectedReturnDateSet()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Инструмент", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Дрель", UserId = userId, CategoryId = category.Id };

            db.Categories.Add(category);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var returnDate = DateTime.UtcNow.AddDays(7);
            var dto = new LendItemDto(
                ItemId: item.Id,
                PersonName: "Иван Иванов",
                ExpectedReturnDate: returnDate,
                Comment: "Без насадок",
                ValueAtLending: 100m,
                ContactEmail: "ivan@test.com",
                SendNotification: true,
                Direction: 0 // Выдать (Lend)
            );

            var handler = new LendItemHandler(db, _sanitizerMock, _trackerMock);
            var command = new LendItemCommand(userId, dto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.PersonName.ShouldBe("Иван Иванов");

            // 1. Статус вещи меняется на Lent
            var itemInDb = await db.Items.FirstAsync(i => i.Id == item.Id);
            itemInDb.Status.ShouldBe(ItemStatus.Lent);

            // 2. Создана запись Lending
            var lendingInDb = await db.Lendings.FirstOrDefaultAsync(l => l.ItemId == item.Id);
            lendingInDb.ShouldNotBeNull();
            lendingInDb.PersonName.ShouldBe("Иван Иванов");

            // 3. Создалась авто-напоминалка на возврат
            var reminder = await db.Reminders.FirstOrDefaultAsync(r => r.ItemId == item.Id);
            reminder.ShouldNotBeNull();
            reminder.Type.ShouldBe(ReminderType.ReturnItem);

            // 4. Запись истории
            var history = await db.ItemHistories.FirstOrDefaultAsync(h => h.ItemId == item.Id);
            history.ShouldNotBeNull();

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task LendItem_ShouldBorrowItem_WhenDirectionIsOne()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Техника", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Нивелир", UserId = userId, CategoryId = category.Id };

            db.Categories.Add(category);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var dto = new LendItemDto(
                ItemId: item.Id,
                PersonName: "Сосед кум",
                ExpectedReturnDate: null,
                Comment: null,
                ValueAtLending: null,
                ContactEmail: null,
                SendNotification: false,
                Direction: 1 // Взять во временное пользование (Borrow)
            );

            var handler = new LendItemHandler(db, _sanitizerMock, _trackerMock);

            // Act
            await handler.Handle(new LendItemCommand(userId, dto), CancellationToken.None);

            // Assert
            var itemInDb = await db.Items.FirstAsync(i => i.Id == item.Id);
            itemInDb.Status.ShouldBe(ItemStatus.Borrowed);
        }

        [Fact]
        public async Task LendItem_ShouldThrowInvalidOperationException_WhenAlreadyLent()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Разное", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Палатка", UserId = userId, CategoryId = category.Id };

            var existingLending = new Lending
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = userId,
                PersonName = "Пётр",
                ReturnedDate = null
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Lendings.Add(existingLending);
            await db.SaveChangesAsync();

            var dto = new LendItemDto(
                ItemId: item.Id,
                PersonName: "Василий",
                ExpectedReturnDate: null,
                Comment: null,
                ValueAtLending: null,
                ContactEmail: null,
                SendNotification: false,
                Direction: 0
            );

            var handler = new LendItemHandler(db, _sanitizerMock, _trackerMock);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await handler.Handle(new LendItemCommand(userId, dto), CancellationToken.None);
            });
        }

        #endregion

        #region ReturnItemHandler Tests

        [Fact]
        public async Task ReturnItem_ShouldReturnOwnLentItem_AndRemoveLendingRecord()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Спорт", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Велосипед", UserId = userId, CategoryId = category.Id };
            item.Lend();

            var lending = new Lending
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = userId,
                PersonName = "Сергей"
            };

            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = userId,
                Type = ReminderType.ReturnItem
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Lendings.Add(lending);
            db.Reminders.Add(reminder);
            await db.SaveChangesAsync();

            var handler = new ReturnItemHandler(db, _trackerMock);
            var returnDto = new ReturnItemDto(DateTime.UtcNow);
            var command = new ReturnItemCommand(userId, item.Id, returnDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            // 1. Предмет снова Active
            var itemInDb = await db.Items.FirstAsync(i => i.Id == item.Id);
            itemInDb.Status.ShouldBe(ItemStatus.Active);

            // 2. Запись о выдаче и напоминание удалены
            (await db.Lendings.FirstOrDefaultAsync(l => l.Id == lending.Id)).ShouldBeNull();
            (await db.Reminders.FirstOrDefaultAsync(r => r.Id == reminder.Id)).ShouldBeNull();

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task ReturnItem_ShouldDeleteBorrowedItem_WhenReturned()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Стройка", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Чужая бетономешалка", UserId = userId, CategoryId = category.Id };
            item.Borrow();

            var lending = new Lending
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                UserId = userId,
                PersonName = "Хозяин бетономешалки"
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Lendings.Add(lending);
            await db.SaveChangesAsync();

            var handler = new ReturnItemHandler(db, _trackerMock);
            var returnDto = new ReturnItemDto(DateTime.UtcNow);
            var command = new ReturnItemCommand(userId, item.Id, returnDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            (await db.Items.FirstOrDefaultAsync(i => i.Id == item.Id)).ShouldBeNull();
            (await db.Lendings.FirstOrDefaultAsync(l => l.Id == lending.Id)).ShouldBeNull();
        }

        #endregion
    }
}