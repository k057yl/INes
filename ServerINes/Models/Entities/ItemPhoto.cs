namespace INest.Models.Entities
{
    public class ItemPhoto
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }

        public string FilePath { get; set; } = null!;
        public bool IsMain { get; set; }

        public DateTime UploadedAt { get; set; }

        public Item Item { get; set; } = null!;
    }
}
