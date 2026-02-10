using INest.Models.Enums;

namespace INest.Models.Entities
{
    public class ItemHistory
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }

        public ItemHistoryType Type { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public Item Item { get; set; } = null!;
    }
}
