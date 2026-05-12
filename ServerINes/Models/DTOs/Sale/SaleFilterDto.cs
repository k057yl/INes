using INest.Models.Enums;

namespace INest.Models.DTOs.Sale
{
    public class SaleFilterDto
    {
        public string? SearchQuery { get; set; }
        public Guid? PlatformId { get; set; }
        public Guid? CategoryId { get; set; }
        public SaleSortOption SortBy { get; set; } = SaleSortOption.DateDesc;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinProfit { get; set; }
        public decimal? MaxProfit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
