using INest.Data.Entities.Core;

namespace INest.Data.Entities.Finances
{
    public class Sale : AuditableEntity
    {
        public Guid? ItemId { get; set; }

        public string ItemNameSnapshot { get; set; } = null!;

        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? CategoryNameSnapshot { get; set; }

        public decimal SalePrice { get; set; }
        public decimal PurchasePriceSnapshot { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal Profit { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime SoldDate { get; set; }

        public Guid? PlatformId { get; set; }
        public Platform? Platform { get; set; }
        public string? Comment { get; set; }
    }
}