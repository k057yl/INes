using INest.Data.Enums;

namespace INest.Data.Entities.Infrastructure
{
    public class Feedback
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; } = null!;

        public FeedbackType Type { get; set; }
        public string Message { get; set; } = string.Empty;

        public int? Rating { get; set; }
        public string? MissingFeatures { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsProcessed { get; set; } = false;
    }
}
