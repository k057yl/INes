using INest.Data.Entities.Core;
using INest.Data.Entities.Infrastructure;
using INest.Infrastructure.BackgroundServices.Reminder;
using INest.Infrastructure.Dispatcher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
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

    public class ReminderWorkerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSendNotification_WhenTimeIsBetween9And10AM()
        {
            using var context = GetInMemoryDbContext();
            var fakeDispatcher = new FakeNotificationDispatcher();
            var userId = Guid.NewGuid();
            var currentUtcHour = DateTime.UtcNow.Hour;
            var targetOffset = 9 - currentUtcHour;
            var offsetSign = targetOffset >= 0 ? "+" : "-";
            var customTzId = $"UTC{offsetSign}{Math.Abs(targetOffset):D2}:00";

            var user = new AppUser
            {
                Id = userId,
                UserName = "testuser",
                DisplayName = "Test User",
                Email = "testuser@example.com",
                NormalizedEmail = "TESTUSER@EXAMPLE.COM",
                NormalizedUserName = "TESTUSER",
                TimeZoneId = "UTC"
            };

            try
            {
                var targetTime = TimeSpan.FromHours(9.5);
                var now = DateTime.UtcNow;

                foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    var localTime = TimeZoneInfo.ConvertTimeFromUtc(now, tz);
                    if (localTime.Hour == 9)
                    {
                        user.TimeZoneId = tz.Id;
                        break;
                    }
                }
            }
            catch { }

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
                TriggerAt = DateTime.UtcNow.AddDays(-1),
                IsCompleted = false,
                IsNotificationSent = false,
                SendTelegram = true
            };

            context.Users.Add(user);
            context.Items.Add(item);
            context.Reminders.Add(reminder);
            await context.SaveChangesAsync();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped(_ => context);
            serviceCollection.AddScoped<INotificationDispatcher>(_ => fakeDispatcher);
            serviceCollection.AddScoped<IStringLocalizer<SharedResource>>(_ => new FakeStringLocalizer());
            serviceCollection.AddLogging();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<ReminderWorker>>();

            var worker = new ReminderWorker(serviceProvider, logger);
            using var cts = new CancellationTokenSource();

            var workerTask = worker.StartAsync(cts.Token);
            await Task.Delay(300);
            await worker.StopAsync(CancellationToken.None);

            fakeDispatcher.WasCalled.ShouldBeTrue();
            fakeDispatcher.LastUserId.ShouldBe(userId);
        }
    }
}