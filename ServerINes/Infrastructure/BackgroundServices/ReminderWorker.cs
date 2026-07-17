using INest.Data.Enums;
using INest.Infrastructure.Dispatcher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static INest.Constants.LocalizationConstants;

namespace INest.Infrastructure.BackgroundServices
{
    public class ReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ReminderWorker> _logger;

        public ReminderWorker(IServiceProvider services, ILogger<ReminderWorker> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Worker: Запуск единого центра уведомлений.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nowUtc = DateTime.UtcNow;

                    using (var scope = _services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
                        var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();

                        var pendingReminders = await context.Reminders
                            .Include(r => r.Item)
                            .ThenInclude(i => i.User)
                            .Where(r => !r.IsCompleted && !r.IsNotificationSent)
                            .Where(r => r.TriggerAt <= nowUtc.AddDays(2))
                            .ToListAsync(stoppingToken);

                        if (pendingReminders.Any())
                        {
                            foreach (var reminder in pendingReminders)
                            {
                                var user = reminder.Item?.User;
                                if (user == null) continue;

                                if (IsUserMorningTime(user, nowUtc))
                                {
                                    var messageTemplate = localizer["TG_NOTIFICATION_REMINDER_ALERT"].Value;
                                    var message = string.Format(messageTemplate, reminder.Title, reminder.Item!.Name);

                                    await dispatcher.SendAsync(user.Id, message, EMAILS.REMINDER_SUBJECT, EMAILS.REMINDER_BODY, stoppingToken);

                                    var recurrence = (ReminderRecurrence)reminder.Recurrence;

                                    if (recurrence == ReminderRecurrence.None)
                                    {
                                        reminder.IsNotificationSent = true;
                                        reminder.IsCompleted = true;
                                    }
                                    else
                                    {
                                        reminder.TriggerAt = recurrence switch
                                        {
                                            ReminderRecurrence.Daily => reminder.TriggerAt.AddDays(1),
                                            ReminderRecurrence.Weekly => reminder.TriggerAt.AddDays(7),
                                            ReminderRecurrence.Monthly => reminder.TriggerAt.AddMonths(1),
                                            ReminderRecurrence.Yearly => reminder.TriggerAt.AddYears(1),
                                            _ => reminder.TriggerAt
                                        };
                                        reminder.IsNotificationSent = false;
                                    }
                                }
                            }
                        }

                        var expiringItems = await context.Items
                            .Include(i => i.User)
                            .Include(i => i.Details)
                            .Where(i => i.Details != null && i.Details.WarrantyExpiration != null && !i.Details.WarrantyAlertSent)
                            .Where(i => i.Details!.WarrantyExpiration.Value <= nowUtc.AddDays(32))
                            .ToListAsync(stoppingToken);

                        foreach (var item in expiringItems)
                        {
                            if (item.User == null || item.Details == null) continue;

                            if (IsUserMorningTime(item.User, nowUtc))
                            {
                                var userTzId = string.IsNullOrEmpty(item.User.TimeZoneId) ? "UTC" : item.User.TimeZoneId;
                                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(userTzId);
                                var userLocalNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tzInfo);
                                var userLocalWarranty = TimeZoneInfo.ConvertTimeFromUtc(item.Details.WarrantyExpiration!.Value, tzInfo);

                                if ((userLocalWarranty.Date - userLocalNow.Date).Days <= 30)
                                {
                                    var messageTemplate = localizer["TG_WARRANTY_EXPIRING_ALERT"].Value;
                                    var message = string.Format(messageTemplate, item.Name);

                                    await dispatcher.SendAsync(item.User.Id, message, EMAILS.LENDING_SUBJECT, EMAILS.LENDING_BODY, stoppingToken);

                                    item.Details.WarrantyAlertSent = true;
                                }
                            }
                        }

                        if (context.ChangeTracker.HasChanges())
                        {
                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в цикле единого ReminderWorker");
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        private bool IsUserMorningTime(dynamic user, DateTime nowUtc)
        {
            try
            {
                var userTzId = string.IsNullOrEmpty(user.TimeZoneId) ? "UTC" : user.TimeZoneId;
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(userTzId);
                var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tzInfo);

                var startTime = new TimeSpan(9, 0, 0);
                var endTime = new TimeSpan(10, 0, 0);

                return userLocalTime.TimeOfDay >= startTime && userLocalTime.TimeOfDay < endTime;
            }
            catch
            {
                return false;
            }
        }
    }
}