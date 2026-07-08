using INest.Data.Entities.Infrastructure;

namespace INest.Data.Entities.Core
{
    public class Category : AuditableEntity
    {
        public AppUser User { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string Color { get; set; } = null!;

        public Guid? ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; } = null!;
        public ICollection<Category> Children { get; set; } = null!;
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
