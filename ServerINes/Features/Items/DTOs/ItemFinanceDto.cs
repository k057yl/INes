namespace INest.Features.Items.DTOs
{
    public class ItemFinanceDto
    {
        public decimal? PurchasePrice { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiration { get; set; }
        public string? ReceiptDocumentPath { get; set; }
        public IFormFile? ReceiptFile { get; set; }
    }
}
