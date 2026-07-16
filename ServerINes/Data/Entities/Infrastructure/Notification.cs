namespace INest.Data.Entities.Infrastructure
{
    public class Notification : AuditableEntity
    {
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;

        public AppUser? User { get; set; }
    }
}
