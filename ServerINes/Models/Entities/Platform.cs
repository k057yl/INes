namespace INest.Models.Entities
{
    public class Platform
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Color { get; set; } = "#00f5d4";

        public AppUser User { get; set; } = null!;
    }
}
