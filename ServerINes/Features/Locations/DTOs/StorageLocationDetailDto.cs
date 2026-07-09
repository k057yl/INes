using INest.Data.Entities.Core;

namespace INest.Features.Locations.DTOs
{
    public class StorageLocationDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        public bool IsSalesLocation { get; set; }
        public bool IsLendingLocation { get; set; }


        public int ItemsCount { get; set; }
        public List<StorageLocation> Children { get; set; } = new();
        public List<Item> Items { get; set; } = new();
    }
}
