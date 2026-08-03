using INest.Data.Entities.Core;
using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;
using INest.Exceptions;
using INest.Features.Reminders.Commands.AddReminder;
using INest.Features.Reminders.Commands.CompleteReminder;
using INest.Features.Reminders.Commands.DeleteReminder;
using INest.Features.Reminders.DTOs;
using INest.Features.Reminders.Services;
using INest.Infrastructure.Sanitizer;
using INest.Infrastructure.Tracker;
using INest.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Features.Reminders
{
    public class RemindersHandlersTests
    {
        private readonly ISanitizerService _sanitizerMock = Substitute.For<ISanitizerService>();
        private readonly ICacheTracker _trackerMock = Substitute.For<ICacheTracker>();
        private readonly IReminderScheduler _schedulerMock = Substitute.For<IReminderScheduler>();

        public RemindersHandlersTests()
        {
            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns(x => x.Arg<string>()?.Trim());
        }

        #region AddReminderHandler Tests

        [Fact]
        public async Task AddReminder_ShouldCreateReminder_AndFallbackReturnItemTypeToCustom()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Инструмент", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Перфоратор", UserId = userId, CategoryId = category.Id };

            db.Categories.Add(category);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            var dto = new CreateReminderDto(
                ItemId: item.Id,
                Title: "Проверить зажим",
                Type: ReminderType.ReturnItem,
                Recurrence: ReminderRecurrence.Monthly,
                TriggerAt: DateTime.UtcNow.AddDays(7),
                SendNotification: true,
                SendTelegram: true
            );

            var handler = new AddReminderHandler(db, _sanitizerMock, _trackerMock);
            var command = new AddReminderCommand(userId, dto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Type.ShouldBe(ReminderType.Custom);
            result.Title.ShouldBe("Проверить зажим");

            var reminderInDb = await db.Reminders.FirstOrDefaultAsync(r => r.Id == result.Id);
            reminderInDb.ShouldNotBeNull();
            reminderInDb.UserId.ShouldBe(userId);

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        [Fact]
        public async Task AddReminder_ShouldThrowKeyNotFoundException_WhenItemDoesNotExist()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var dto = new CreateReminderDto(
                ItemId: Guid.NewGuid(),
                Title: "Тест",
                Type: ReminderType.Custom,
                Recurrence: ReminderRecurrence.None,
                TriggerAt: DateTime.UtcNow.AddDays(1)
            );

            var handler = new AddReminderHandler(db, _sanitizerMock, _trackerMock);

            // Act & Assert
            await Should.ThrowAsync<KeyNotFoundException>(async () =>
            {
                await handler.Handle(new AddReminderCommand(Guid.NewGuid(), dto), CancellationToken.None);
            });
        }

        [Fact]
        public async Task AddReminder_ShouldThrowAppException_WhenTitleIsInvalidAfterSanitizing()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Дрель", UserId = userId, CategoryId = category.Id };

            db.Categories.Add(category);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            _sanitizerMock.StripAllHtml(Arg.Any<string>()).Returns("");

            var dto = new CreateReminderDto(
                ItemId: item.Id,
                Title: "<script></script>",
                Type: ReminderType.Custom,
                Recurrence: ReminderRecurrence.None,
                TriggerAt: DateTime.UtcNow.AddDays(1)
            );

            var handler = new AddReminderHandler(db, _sanitizerMock, _trackerMock);

            // Act & Assert
            await Should.ThrowAsync<AppException>(async () =>
            {
                await handler.Handle(new AddReminderCommand(userId, dto), CancellationToken.None);
            });
        }

        #endregion

        #region CompleteReminderHandler Tests

        [Fact]
        public async Task CompleteReminder_ShouldMarkCompleted_CreateHistory_AndSpawnNext_IfRecurring()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Болгарка", UserId = userId, CategoryId = category.Id };

            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                Item = item,
                Title = "Заменить диск",
                Recurrence = ReminderRecurrence.Monthly,
                IsCompleted = false
            };

            var nextReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                Title = "Заменить диск",
                TriggerAt = DateTime.UtcNow.AddMonths(1)
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Reminders.Add(reminder);
            await db.SaveChangesAsync();

            _schedulerMock.CreateNext(Arg.Any<Reminder>(), Arg.Any<DateTime>()).Returns(nextReminder);

            var handler = new CompleteReminderHandler(db, _trackerMock, _schedulerMock);
            var command = new CompleteReminderCommand(userId, reminder.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            var completedInDb = await db.Reminders.FirstAsync(r => r.Id == reminder.Id);
            completedInDb.IsCompleted.ShouldBeTrue();

            var nextInDb = await db.Reminders.FirstOrDefaultAsync(r => r.Id == nextReminder.Id);
            nextInDb.ShouldNotBeNull();

            var history = await db.ItemHistories.FirstOrDefaultAsync(h => h.ItemId == item.Id);
            history.ShouldNotBeNull();
            history.Type.ShouldBe(ItemHistoryType.ReminderCompleted);

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        #endregion

        #region DeleteReminderHandler Tests

        [Fact]
        public async Task DeleteReminder_ShouldRemoveFromDbSuccessfully()
        {
            // Arrange
            using var db = DbContextFactory.Create();
            var userId = Guid.NewGuid();

            var category = new Category { Id = Guid.NewGuid(), Name = "Тест", Color = "#00B894", UserId = userId };
            var item = new Item { Id = Guid.NewGuid(), Name = "Шлифмашина", UserId = userId, CategoryId = category.Id };

            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                Item = item,
                Title = "Проверить кабель"
            };

            db.Categories.Add(category);
            db.Items.Add(item);
            db.Reminders.Add(reminder);
            await db.SaveChangesAsync();

            var handler = new DeleteReminderHandler(db, _trackerMock);
            var command = new DeleteReminderCommand(userId, reminder.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldBeTrue();

            var deletedInDb = await db.Reminders.FirstOrDefaultAsync(r => r.Id == reminder.Id);
            deletedInDb.ShouldBeNull();

            _trackerMock.Received(1).InvalidateUserCache(userId);
        }

        #endregion
    }
}