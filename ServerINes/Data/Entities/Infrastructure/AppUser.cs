using INest.Data.Entities.Core;
using INest.Data.Enums;
using Microsoft.AspNetCore.Identity;

namespace INest.Data.Entities.Infrastructure
{
    public class AppUser : IdentityUser<Guid>
    {
        public string DisplayName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiryTime { get; set; }

        public ICollection<StorageLocation> Locations { get; set; } = new List<StorageLocation>();
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public string TimeZoneId { get; set; } = "Europe/Kyiv";

        public long? TelegramChatId { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public TutorialSteps CompletedTutorials { get; set; } = TutorialSteps.None;
    }
}