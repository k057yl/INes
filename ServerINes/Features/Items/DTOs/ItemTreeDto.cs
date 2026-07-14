using INest.Data.Enums;

namespace INest.Features.Items.DTOs
{
    public class ItemTreeDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ItemStatus Status { get; set; }

        public decimal? PurchasePrice { get; set; }

        public decimal? EstimatedValue { get; set; }

        public string Currency { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
    }
}
