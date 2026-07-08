namespace INest.Data.Entities.Core
{
    public class ItemPhoto : AuditableEntity
    {
        public Guid ItemId { get; set; }

        public string FilePath { get; set; } = null!;
        public string? PublicId { get; set; }
        public bool IsMain { get; set; }

        public DateTime UploadedAt { get; set; }

        public Item Item { get; set; } = null!;
    }
}
