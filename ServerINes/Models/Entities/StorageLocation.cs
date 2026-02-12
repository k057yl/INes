namespace INest.Models.Entities
{
    public class StorageLocation
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public string? Color { get; set; }
        public string? Icon { get; set; }

        public Guid? ParentLocationId { get; set; }
        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public AppUser User { get; set; } = null!;
        public StorageLocation? ParentLocation { get; set; }
        public ICollection<StorageLocation> Children { get; set; } = new List<StorageLocation>();
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
