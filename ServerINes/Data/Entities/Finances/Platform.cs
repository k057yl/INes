using INest.Data.Entities.Infrastructure;

namespace INest.Data.Entities.Finances
{
    public class Platform : AuditableEntity
    {
        public string Name { get; set; } = null!;
        public string Color { get; set; } = "#00f5d4";

        public AppUser User { get; set; } = null!;
    }
}
