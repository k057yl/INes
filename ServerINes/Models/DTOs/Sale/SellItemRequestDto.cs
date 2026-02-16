using System.ComponentModel.DataAnnotations;

namespace INest.Models.DTOs.Sale
{
    public class SellItemRequestDto
    {
        [Required]
        public Guid ItemId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal SalePrice { get; set; }

        public DateTime SoldDate { get; set; } = DateTime.UtcNow;

        public Guid? PlatformId { get; set; }

        public string? Comment { get; set; }
    }
}
