using INest.Data.Entities.Infrastructure;
using INest.Data.Enums;

namespace INest.Data.Entities.Core
{
    public class Item : AuditableEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ItemStatus Status { get; set; }


        public Guid? StorageLocationId { get; set; }
        public StorageLocation? StorageLocation { get; set; }


        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public AppUser User { get; set; } = null!;

        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime? PurchaseDate { get; set; }

        public string? PhotoUrl { get; set; }
        public string? PublicId { get; set; }
        public ICollection<ItemPhoto> Photos { get; set; } = new List<ItemPhoto>();


        public ICollection<ItemHistory> History { get; set; } = new List<ItemHistory>();
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }
}
