using INest.Features.Items.DTOs;

namespace INest.Features.Locations.DTOs
{
    public class StorageLocationTreeDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Color { get; set; }

        public string? Icon { get; set; }

        public Guid? ParentLocationId { get; set; }

        public int SortOrder { get; set; }

        public bool IsSalesLocation { get; set; }

        public bool IsLendingLocation { get; set; }

        public List<ItemTreeDto> Items { get; set; } = [];

        public List<StorageLocationTreeDto> Children { get; set; } = [];
    }
}
