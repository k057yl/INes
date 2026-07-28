using INest.Data.Entities.Infrastructure;

namespace INest.Features.Reminders.Services
{
    public interface IReminderScheduler
    {
        Reminder? CreateNext(Reminder currentReminder, DateTime nowUtc);
    }
}
