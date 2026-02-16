using INest.Models.Enums;

namespace INest.Models.Entities
{
    public class Item
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ItemStatus Status { get; set; }
        public Guid? StorageLocationId { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }

        public DateTime CreatedAt { get; set; }

        public AppUser User { get; set; } = null!;
        public StorageLocation? StorageLocation { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ItemPhoto> Photos { get; set; } = new List<ItemPhoto>();
        public ICollection<ItemHistory> History { get; set; } = new List<ItemHistory>();
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
        public Lending? Lending { get; set; }
        public string? PhotoUrl { get; set; }
        public string? PublicId { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime? SoldDate { get; set; }
    }
}
