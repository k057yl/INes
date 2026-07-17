namespace INest.Data.Entities.Core
{
    public class ItemDetails
    {
        public Guid Id { get; set; }

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime? PurchaseDate { get; set; }

        public DateTime? WarrantyExpiration { get; set; }
        public bool WarrantyAlertSent { get; set; }
        public string? ReceiptDocumentPath { get; set; }
    }
}
