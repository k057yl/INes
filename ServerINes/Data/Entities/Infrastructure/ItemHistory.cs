using INest.Data.Entities.Core;
using INest.Models.Enums;

namespace INest.Data.Entities.Infrastructure
{
    public class ItemHistory : AuditableEntity
    {
        public Guid ItemId { get; set; }

        public ItemHistoryType Type { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Comment { get; set; }

        public Item Item { get; set; } = null!;
    }
}
