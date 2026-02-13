using INest.Models.DTOs.Item;

namespace INest.Models.DTOs.Location
{
    public class LocationTreeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid? ParentId { get; set; }

        public List<LocationTreeDto> Children { get; set; } = new();
        public List<ItemDto> Items { get; set; } = new();
    }
}
