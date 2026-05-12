using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace INest.Models.Entities
{
    [Index(nameof(UserId))]
    [Index(nameof(CategoryId))]
    public class Sale
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public Guid? ItemId { get; set; }
        public Item? Item { get; set; }

        [Required]
        public string ItemNameSnapshot { get; set; } = null!;

        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? CategoryNameSnapshot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePriceSnapshot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PlatformFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        public DateTime SoldDate { get; set; }

        public Guid? PlatformId { get; set; }
        public Platform? Platform { get; set; }

        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}