using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INest.Models.Entities
{
    public class Sale
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; }
        public DateTime SoldDate { get; set; }
        public Guid? PlatformId { get; set; }
        public StorageLocation? Platform { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
