using INest.Models.Enums;

namespace INest.Models.DTOs.Item
{
    public class ItemFilterDto
    {
        public string? SearchQuery { get; set; }
        public Guid? CategoryId { get; set; }
        public ItemStatus? Status { get; set; }
        public ItemSortOption SortBy { get; set; } = ItemSortOption.Newest;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
