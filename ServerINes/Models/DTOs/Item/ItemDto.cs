using INest.Models.Enums;

namespace INest.Models.DTOs.Item
{
    public class ItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ItemStatus Status { get; set; }

        public decimal? EstimatedValue { get; set; }
        public string Currency { get; set; } = "USD";

        public string? PhotoUrl { get; set; }

        public Guid? StorageLocationId { get; set; }
        public string? StorageLocationName { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public bool IsOverdue { get; set; }
    }
}
