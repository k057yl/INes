using INest.Features.Reminders.Services;
using INest.Infrastructure.Dispatcher;
using INest.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static INest.Constants.LocalizationConstants;

namespace INest.Infrastructure.BackgroundServices.Reminder
{
    public class ReminderProcessor : IReminderProcessor
    {
        private readonly AppDbContext _context;
        private readonly INotificationDispatcher _dispatcher;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IReminderScheduler _scheduler;
        private readonly IUserTimeService _userTimeService;
        private readonly ILogger<ReminderProcessor> _logger;

        public ReminderProcessor(
            AppDbContext context,
            INotificationDispatcher dispatcher,
            IStringLocalizer<SharedResource> localizer,
            IReminderScheduler scheduler,
            IUserTimeService userTimeService,
            ILogger<ReminderProcessor> logger)
        {
            _context = context;
            _dispatcher = dispatcher;
            _localizer = localizer;
            _scheduler = scheduler;
            _userTimeService = userTimeService;
            _logger = logger;
        }

        public async Task ProcessAsync(DateTime nowUtc, CancellationToken stoppingToken)
        {
            var pendingReminders = await _context.Reminders
                .Include(r => r.Item)
                .ThenInclude(i => i.User)
                .Where(r => !r.IsCompleted && !r.IsNotificationSent)
                .Where(r => r.Item != null && r.Item.User != null)
                .Where(r => r.TriggerAt <= nowUtc)
                .ToListAsync(stoppingToken);

            if (!pendingReminders.Any()) return;

            _logger.LogInformation("[ReminderProcessor] Найдено созревших напоминаний: {Count}", pendingReminders.Count);

            foreach (var reminder in pendingReminders)
            {
                var user = reminder.Item!.User!;

                if (!_userTimeService.IsAllowedToNotify(user, nowUtc))
                {
                    continue;
                }

                _logger.LogInformation("[ReminderProcessor] Отправка напоминания Id={Id} для UserId={UserId}", reminder.Id, user.Id);

                var title = reminder.Title;
                var localizedTitle = _localizer[title].ResourceNotFound ? title : _localizer[title].Value;

                var messageTemplate = _localizer["TG_NOTIFICATION_REMINDER_ALERT"].Value;
                var message = string.Format(messageTemplate, localizedTitle, reminder.Item.Name);

                try
                {
                    await _dispatcher.SendAsync(user.Id, message, EMAILS.REMINDER_SUBJECT, EMAILS.REMINDER_BODY, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ReminderProcessor] Ошибка отправки для напоминания Id={Id}", reminder.Id);
                    continue;
                }

                reminder.IsNotificationSent = true;
                reminder.IsCompleted = true;

                var nextReminder = _scheduler.CreateNext(reminder, nowUtc);
                if (nextReminder != null)
                {
                    _context.Reminders.Add(nextReminder);
                }
            }

            await _context.SaveChangesAsync(stoppingToken);
        }
    }
}
