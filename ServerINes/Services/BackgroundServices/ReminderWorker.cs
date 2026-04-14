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
                    using (var scope = _services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        var now = DateTime.UtcNow;
                        var targetDate = now.AddDays(1).Date;

                        if (now.Hour >= 9)
                        {
                            var pendingReminders = await context.Reminders
                                .Include(r => r.Item)
                                .ThenInclude(i => i.User)
                                .Where(r => !r.IsCompleted && !r.IsNotificationSent)
                                .Where(r => r.TriggerAt.Date == targetDate)
                                .ToListAsync(stoppingToken);

                            foreach (var reminder in pendingReminders)
                            {
                                await emailService.SendReminderNotificationAsync(
                                    reminder.Item.User.Email,
                                    reminder.Title,
                                    reminder.TriggerAt);

                                reminder.IsNotificationSent = true;
                                _logger.LogInformation("Уведомление отправлено для: {Title}", reminder.Title);
                            }

                            if (pendingReminders.Any())
                            {
                                await context.SaveChangesAsync(stoppingToken);
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
