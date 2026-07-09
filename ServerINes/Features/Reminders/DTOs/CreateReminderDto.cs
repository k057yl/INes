using INest.Data.Enums;

namespace INest.Features.Reminders.DTOs
{
    public record CreateReminderDto(
        Guid ItemId,
        string Title,
        ReminderType Type,
        ReminderRecurrence Recurrence,
        DateTime TriggerAt
    );
}
