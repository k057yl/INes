using INest.Models.Enums;

namespace INest.Models.Entities
{
    public class Reminder
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }

        public ReminderType Type { get; set; }
        public DateTime TriggerAt { get; set; }
        public bool IsCompleted { get; set; }

        public Item Item { get; set; } = null!;
    }
}
