using INest.Models.Enums;

namespace INest.Models.DTOs.Item
{
    public class UpdateItemFullDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public Guid StorageLocationId { get; set; }
        public ItemStatus Status { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }
    }
}
