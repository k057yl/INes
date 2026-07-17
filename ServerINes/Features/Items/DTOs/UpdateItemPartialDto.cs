using INest.Data.Enums;

namespace INest.Features.Items.DTOs
{
    public class UpdateItemPartialDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? StorageLocationId { get; set; }
        public ItemStatus? Status { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string? Currency { get; set; }

        public DateTime? WarrantyExpiration { get; set; }

        public List<IFormFile>? Photos { get; set; }
        public string? MainPhotoName { get; set; }
    }
}
