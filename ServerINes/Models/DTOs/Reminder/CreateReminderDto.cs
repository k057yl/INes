using INest.Models.Enums;

namespace INest.Models.DTOs.Reminder
{
    public record CreateReminderDto(
        Guid ItemId,
        string Title,
        ReminderType Type,
        ReminderRecurrence Recurrence,
        DateTime TriggerAt
    );
}
