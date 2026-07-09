using INest.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

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
            _logger.LogInformation("Reminder Worker: Начинаем утренний обход планеты (09:00 - 10:00).");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nowUtc = DateTime.UtcNow;

                    using (var scope = _services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
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
                                if (user == null || string.IsNullOrEmpty(user.Email)) continue;

                                var userTzId = string.IsNullOrEmpty(user.TimeZoneId) ? "UTC" : user.TimeZoneId;

                                try
                                {
                                    var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(userTzId);
                                    var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tzInfo);
                                    var startTime = new TimeSpan(9, 0, 0);
                                    var endTime = new TimeSpan(10, 0, 0);
                                    var currentTime = userLocalTime.TimeOfDay;

                                    if (currentTime >= startTime && currentTime < endTime)
                                    {
                                        await emailService.SendReminderNotificationAsync(
                                            user.Email,
                                            reminder.Title,
                                            reminder.TriggerAt);

                                        reminder.IsNotificationSent = true;
                                        _logger.LogInformation("Уведомление отправлено на {Email}. У юзера сейчас: {Time}", user.Email, userLocalTime);
                                    }
                                }
                                catch (TimeZoneNotFoundException)
                                {
                                    _logger.LogWarning("У юзера {Email} какая-то дичь вместо часового пояса: {Tz}", user.Email, userTzId);
                                }
                            }

                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в цикле рассылки");
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}