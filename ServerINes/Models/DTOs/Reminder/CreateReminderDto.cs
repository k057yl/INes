using INest.Models.Enums;

namespace INest.Models.DTOs.Reminder
{
    public record CreateReminderDto(Guid ItemId, ReminderType Type, DateTime TriggerAt);
}
