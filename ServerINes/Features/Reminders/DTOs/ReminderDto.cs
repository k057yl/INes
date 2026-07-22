using INest.Data.Enums;

namespace INest.Features.Reminders.DTOs
{
    public class ReminderDto
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public ReminderType Type { get; set; }
        public ReminderRecurrence Recurrence { get; set; }
        public DateTime TriggerAt { get; set; }
        public bool IsCompleted { get; set; }
    }
}
