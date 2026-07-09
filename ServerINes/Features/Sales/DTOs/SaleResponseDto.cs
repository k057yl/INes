namespace INest.Features.Sales.DTOs
{
    public class SaleResponseDto
    {
        public Guid SaleId { get; set; }
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public decimal SalePrice { get; set; }
        public decimal Profit { get; set; }
        public DateTime SoldDate { get; set; }
        public string? PlatformName { get; set; }
        public string? CategoryName { get; set; }
    }
}
