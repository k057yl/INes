using INest.Data.Entities.Core;
using INest.Data.Entities.Infrastructure;
using INest.Features.Reminders.Services;
using INest.Infrastructure.BackgroundServices.Reminder;
using INest.Infrastructure.Dispatcher;
using INest.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using ReminderEntity = INest.Data.Entities.Infrastructure.Reminder;

namespace INest.Tests.Infrastructure
{
    public class FakeNotificationDispatcher : INotificationDispatcher
    {
        public bool WasCalled { get; private set; }
        public Guid LastUserId { get; private set; }

        public Task SendAsync(Guid userId, string message, string emailSubject, string emailBody, CancellationToken ct = default)
        {
            WasCalled = true;
            LastUserId = userId;
            return Task.CompletedTask;
        }
    }

    public class FakeStringLocalizer : IStringLocalizer<SharedResource>
    {
        public LocalizedString this[string name] => new LocalizedString(name, "Тест {0} {1}");
        public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, "Тест");
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
    }

    public class ReminderProcessorTests
    {
        private readonly IUserTimeService _userTimeServiceMock = Substitute.For<IUserTimeService>();
        private readonly IReminderScheduler _schedulerMock = Substitute.For<IReminderScheduler>();
        private readonly ILogger<ReminderProcessor> _loggerMock = Substitute.For<ILogger<ReminderProcessor>>();

        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task ProcessAsync_ShouldSendNotification_WhenReminderIsDueAndUserIsAllowed()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var fakeDispatcher = new FakeNotificationDispatcher();
            var userId = Guid.NewGuid();
            var nowUtc = DateTime.UtcNow;

            var user = new AppUser
            {
                Id = userId,
                UserName = "testuser",
                DisplayName = "Test User",
                Email = "testuser@example.com",
                TimeZoneId = "UTC"
            };

            var item = new Item
            {
                Id = Guid.NewGuid(),
                Name = "Дрель",
                UserId = userId,
                User = user
            };

            var reminder = new ReminderEntity
            {
                Id = Guid.NewGuid(),
                Title = "Проверить щетки",
                ItemId = item.Id,
                Item = item,
                UserId = userId,
                TriggerAt = nowUtc.AddMinutes(-10),
                IsCompleted = false,
                IsNotificationSent = false
            };

            context.Users.Add(user);
            context.Items.Add(item);
            context.Reminders.Add(reminder);
            await context.SaveChangesAsync();

            _userTimeServiceMock.IsAllowedToNotify(Arg.Any<AppUser>(), Arg.Any<DateTime>()).Returns(true);

            var processor = new ReminderProcessor(
                context,
                fakeDispatcher,
                new FakeStringLocalizer(),
                _schedulerMock,
                _userTimeServiceMock,
                _loggerMock
            );

            // Act
            await processor.ProcessAsync(nowUtc, CancellationToken.None);

            // Assert
            fakeDispatcher.WasCalled.ShouldBeTrue();
            fakeDispatcher.LastUserId.ShouldBe(userId);

            var reminderInDb = await context.Reminders.FirstAsync(r => r.Id == reminder.Id);
            reminderInDb.IsNotificationSent.ShouldBeTrue();
            reminderInDb.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task ProcessAsync_ShouldNotSendNotification_WhenUserIsNotAllowedToNotify()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var fakeDispatcher = new FakeNotificationDispatcher();
            var userId = Guid.NewGuid();
            var nowUtc = DateTime.UtcNow;
            var user = new AppUser
            {
                Id = userId,
                UserName = "testuser",
                DisplayName = "Test User",
                Email = "test@ex.com"
            };

            var item = new Item { Id = Guid.NewGuid(), Name = "Шуруповерт", UserId = userId, User = user };
            var reminder = new ReminderEntity
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                Item = item,
                UserId = userId,
                TriggerAt = nowUtc.AddMinutes(-5),
                IsCompleted = false,
                IsNotificationSent = false
            };

            context.Users.Add(user);
            context.Items.Add(item);
            context.Reminders.Add(reminder);
            await context.SaveChangesAsync();

            _userTimeServiceMock.IsAllowedToNotify(Arg.Any<AppUser>(), Arg.Any<DateTime>()).Returns(false);

            var processor = new ReminderProcessor(
                context,
                fakeDispatcher,
                new FakeStringLocalizer(),
                _schedulerMock,
                _userTimeServiceMock,
                _loggerMock
            );

            // Act
            await processor.ProcessAsync(nowUtc, CancellationToken.None);

            // Assert
            fakeDispatcher.WasCalled.ShouldBeFalse();

            var reminderInDb = await context.Reminders.FirstAsync(r => r.Id == reminder.Id);
            reminderInDb.IsNotificationSent.ShouldBeFalse();
            reminderInDb.IsCompleted.ShouldBeFalse();
        }
    }
}