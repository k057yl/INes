using INest.Models.DTOs.Item;

namespace INest.Models.DTOs.Location
{
    public class LocationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }

        public List<ItemDto> Items { get; set; } = new();
    }
}
