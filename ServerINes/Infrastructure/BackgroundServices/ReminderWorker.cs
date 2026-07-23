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
            _logger.LogInformation("[ReminderWorker] Запуск единого центра уведомлений.");

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

                        // --- 1. ОБРАБОТКА НАПОМИНАНИЙ (REMINDERS) ---
                        var pendingReminders = await context.Reminders
                            .Include(r => r.Item)
                            .ThenInclude(i => i.User)
                            .Where(r => !r.IsCompleted && !r.IsNotificationSent)
                            .Where(r => r.TriggerAt <= nowUtc)
                            .ToListAsync(stoppingToken);

                        if (pendingReminders.Any())
                        {
                            _logger.LogInformation("[ReminderWorker] Найдено созревших напоминаний: {Count}", pendingReminders.Count);

                            foreach (var reminder in pendingReminders)
                            {
                                var user = reminder.Item?.User;
                                if (user == null)
                                {
                                    _logger.LogWarning("[ReminderWorker] Напоминание Id={Id} пропущено: пользователь или предмет отсутствует.", reminder.Id);
                                    continue;
                                }

                                if (IsUserMorningOrLater(user, nowUtc))
                                {
                                    _logger.LogInformation("[ReminderWorker] Попытка отправки напоминания Id={Id} для UserId={UserId}", reminder.Id, user.Id);

                                    var messageTemplate = localizer["TG_NOTIFICATION_REMINDER_ALERT"].Value;
                                    var message = string.Format(messageTemplate, reminder.Title, reminder.Item!.Name);

                                    try
                                    {
                                        await dispatcher.SendAsync(user.Id, message, EMAILS.REMINDER_SUBJECT, EMAILS.REMINDER_BODY, stoppingToken);
                                        _logger.LogInformation("[ReminderWorker] Напоминание Id={Id} успешно передано в Dispatcher.", reminder.Id);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "[ReminderWorker] Ошибка при вызове SendAsync в Dispatcher для напоминания Id={Id}", reminder.Id);
                                        continue;
                                    }

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

                        // --- 2. ОБРАБОТКА ИСТЕКАЮЩИХ ГАРАНТИЙ (WARRANTY) ---
                        var expiringItems = await context.Items
                            .Include(i => i.User)
                            .Include(i => i.Details)
                            .Where(i => i.Details != null && i.Details.WarrantyExpiration != null && !i.Details.WarrantyAlertSent)
                            .Where(i => i.Details!.WarrantyExpiration.Value <= nowUtc.AddDays(30))
                            .ToListAsync(stoppingToken);

                        foreach (var item in expiringItems)
                        {
                            if (item.User == null || item.Details == null) continue;

                            if (IsUserMorningOrLater(item.User, nowUtc))
                            {
                                _logger.LogInformation("[ReminderWorker] Попытка отправки алерта по гарантии ItemId={ItemId} для UserId={UserId}", item.Id, item.User.Id);

                                var messageTemplate = localizer["TG_WARRANTY_EXPIRING_ALERT"].Value;
                                var message = string.Format(messageTemplate, item.Name);

                                try
                                {
                                    await dispatcher.SendAsync(item.User.Id, message, EMAILS.LENDING_SUBJECT, EMAILS.LENDING_BODY, stoppingToken);
                                    _logger.LogInformation("[ReminderWorker] Алерт по гарантии ItemId={ItemId} успешно передан в Dispatcher.", item.Id);

                                    item.Details.WarrantyAlertSent = true;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "[ReminderWorker] Ошибка при вызове SendAsync для гарантии ItemId={ItemId}", item.Id);
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
                    _logger.LogError(ex, "Критическая ошибка в цикле ReminderWorker");
                }

                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }

        private bool IsUserMorningOrLater(dynamic user, DateTime nowUtc)
        {
            try
            {
                string userTzId = user.TimeZoneId;
                if (string.IsNullOrWhiteSpace(userTzId))
                {
                    userTzId = "UTC";
                }

                TimeZoneInfo? tzInfo = null;

                if (!TimeZoneInfo.TryFindSystemTimeZoneById(userTzId, out tzInfo))
                {
                    if (OperatingSystem.IsWindows())
                    {
                        var winId = userTzId switch
                        {
                            "Europe/Kyiv" or "Europe/Kiev" => "FLE Standard Time",
                            "Europe/Warsaw" => "Central European Standard Time",
                            "Europe/London" => "GMT Standard Time",
                            "America/New_York" => "Eastern Standard Time",
                            _ => null
                        };

                        if (winId != null)
                        {
                            TimeZoneInfo.TryFindSystemTimeZoneById(winId, out tzInfo);
                        }

                        if (tzInfo == null && TimeZoneInfo.TryConvertIanaIdToWindowsId(userTzId, out var convertedWinId))
                        {
                            TimeZoneInfo.TryFindSystemTimeZoneById(convertedWinId, out tzInfo);
                        }
                    }
                }

                tzInfo ??= TimeZoneInfo.Utc;

                var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tzInfo);

                _logger.LogInformation(
                    "[ReminderWorker] Проверка времени: UserId={UserId}, TzId={TzId}, ResolvedTz={ResolvedTz}, LocalTime={LocalTime}, Hour={Hour}",
                    (object)user.Id, userTzId, tzInfo.Id, userLocalTime.ToString("yyyy-MM-dd HH:mm:ss"), userLocalTime.Hour);

                return userLocalTime.Hour >= 9;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReminderWorker] Ошибка при расчете локального времени пользователя.");
                return false;
            }
        }
    }
}