namespace INest.Models.DTOs.Location
{
    public class CreateLocationDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? ParentLocationId { get; set; }
        public int SortOrder { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool IsSalesLocation { get; set; } = false;
        public bool IsLendingLocation { get; set; } = false;
    }
}
