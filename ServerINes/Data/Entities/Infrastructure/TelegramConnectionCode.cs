namespace INest.Data.Entities.Infrastructure
{
    public class TelegramConnectionCode : AuditableEntity
    {
        public string Code { get; set; } = null!;
        public DateTime ExpiryTime { get; set; }
    }
}
