using INest.Data.Enums;

namespace INest.Features.Items.DTOs
{
    public class ItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ItemStatus Status { get; set; }

        public decimal? EstimatedValue { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal? PurchasePrice { get; set; }

        public string? PhotoUrl { get; set; }

        public Guid? StorageLocationId { get; set; }
        public string? StorageLocationName { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public bool IsOverdue { get; set; }
        public string? PersonName { get; set; }
        public string? ContactEmail { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public bool IsLendingOverdue { get; set; }
        public bool HasOverdueReminders { get; set; }
    }
}
