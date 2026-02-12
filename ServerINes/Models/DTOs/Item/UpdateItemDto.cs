namespace INest.Models.DTOs.Item
{
    public class UpdateItemDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? StorageLocationId { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }
    }
}
