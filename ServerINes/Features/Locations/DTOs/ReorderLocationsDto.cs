namespace INest.Features.Locations.DTOs
{
    public class ReorderLocationsDto
    {
        public Guid? ParentId { get; set; }
        public List<Guid> OrderedIds { get; set; } = new();
    }
}
