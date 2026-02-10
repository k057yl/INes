using Microsoft.AspNetCore.Identity;

namespace INest.Models.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? VerificationCode { get; set; }

        public ICollection<StorageLocation> Locations { get; set; } = new List<StorageLocation>();
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}