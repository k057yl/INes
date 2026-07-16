using INest.Data.Enums;

namespace INest.Features.Items.DTOs
{
    public class ItemFilterDto
    {
        public string? SearchQuery { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? StorageLocationId { get; set; }
        public ItemStatus? Status { get; set; }
        public ItemSortOption SortBy { get; set; } = ItemSortOption.Newest;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool ShowArchived { get; set; } = false;
        public bool IncludeArchived { get; set; } = false;
    }
}
