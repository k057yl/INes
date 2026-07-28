namespace INest.Features.Locations.DTOs
{
    public class LocationChildDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int ItemsCount { get; set; }
    }
}
