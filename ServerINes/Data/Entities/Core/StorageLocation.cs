using INest.Data.Entities.Infrastructure;

namespace INest.Data.Entities.Core
{
    public class StorageLocation : AuditableEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }


        public Guid? ParentLocationId { get; set; }
        public int SortOrder { get; set; }

        public bool IsSalesLocation { get; set; } = false;
        public bool IsLendingLocation { get; set; } = false;


        public ICollection<Item> Items { get; set; } = new List<Item>();

        public AppUser User { get; set; } = null!;
    }
}
