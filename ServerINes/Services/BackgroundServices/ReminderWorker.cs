using INest.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace INest.Services.BackgroundServices
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
            _logger.LogInformation("Reminder Worker стартовал.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    if (now.Hour == 9)
                    {
                        _logger.LogInformation("Наступило время рассылки (9:00 UTC). Проверяем напоминания...");

                        using (var scope = _services.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                            var targetDate = now.AddDays(1).Date;

                            var pendingReminders = await context.Reminders
                                .Include(r => r.Item)
                                .ThenInclude(i => i.User)
                                .Where(r => !r.IsCompleted && !r.IsNotificationSent)
                                .Where(r => r.TriggerAt.Date <= targetDate)
                                .ToListAsync(stoppingToken);

                            if (pendingReminders.Any())
                            {
                                foreach (var reminder in pendingReminders)
                                {
                                    var toEmail = reminder.Item?.User?.Email;

                                    if (string.IsNullOrEmpty(toEmail))
                                    {
                                        _logger.LogWarning("Пропуск: Email владельца не найден для напоминания {Id}", reminder.Id);
                                        continue;
                                    }

                                    await emailService.SendReminderNotificationAsync(
                                        toEmail,
                                        reminder.Title,
                                        reminder.TriggerAt);

                                    reminder.IsNotificationSent = true;
                                    _logger.LogInformation("Уведомление отправлено на {Email}: {Title}", toEmail, reminder.Title);
                                }

                                await context.SaveChangesAsync(stoppingToken);
                            }
                            else
                            {
                                _logger.LogInformation("На сегодня новых напоминаний нет.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в работе воркера напоминаний");
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}
