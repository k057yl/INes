using INest.Models.Enums;

namespace INest.Models.DTOs.Item
{
    public class ItemFilterDto
    {
        public string? SearchQuery { get; set; }
        public Guid? CategoryId { get; set; }
        public ItemStatus? Status { get; set; }
        public string? SortBy { get; set; }
    }
}
