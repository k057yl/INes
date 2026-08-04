using INest.Data.Entities.Core;

namespace INest.Data.Entities.Finances
{
    public class Lending : AuditableEntity
    {
        public Guid ItemId { get; set; }

        public string PersonName { get; set; } = null!;

        public DateTime DateGiven { get; set; }

        public DateTime? ExpectedReturnDate { get; set; }

        public DateTime? ReturnedDate { get; set; }

        public string? ContactEmail { get; set; }

        public bool SendNotification { get; set; }

        public bool NotificationSent { get; set; }

        public string? Comment { get; set; }

        public Item Item { get; set; } = null!;
    }
}
