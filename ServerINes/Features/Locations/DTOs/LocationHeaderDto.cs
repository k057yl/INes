namespace INest.Features.Locations.DTOs
{
    public class LocationHeaderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public ParentLocationDto? ParentLocation { get; set; }
    }
}
