using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;

namespace INest.Features.Reminders.Services
{
    public class ReminderScheduler : IReminderScheduler
    {
        public Reminder? CreateNext(Reminder currentReminder, DateTime nowUtc)
        {
            var recurrence = (ReminderRecurrence)currentReminder.Recurrence;
            if (recurrence == ReminderRecurrence.None) return null;

            var baseDate = currentReminder.TriggerAt < nowUtc ? nowUtc : currentReminder.TriggerAt;

            DateTime nextTrigger = recurrence switch
            {
                ReminderRecurrence.Daily => baseDate.AddDays(1),
                ReminderRecurrence.Weekly => baseDate.AddDays(7),
                ReminderRecurrence.Monthly => baseDate.AddMonths(1),
                ReminderRecurrence.Yearly => baseDate.AddYears(1),
                _ => baseDate
            };

            return new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = currentReminder.UserId,
                ItemId = currentReminder.ItemId,
                Title = currentReminder.Title,
                Type = currentReminder.Type,
                Recurrence = currentReminder.Recurrence,
                TriggerAt = nextTrigger,
                SendNotification = currentReminder.SendNotification,
                SendTelegram = currentReminder.SendTelegram,
                IsCompleted = false,
                IsNotificationSent = false
            };
        }
    }
}
