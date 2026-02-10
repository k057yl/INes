namespace INest.Models.Entities
{
    public class Lending
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }

        public string PersonName { get; set; } = null!;
        public DateTime DateGiven { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        public string? Comment { get; set; }

        public Item Item { get; set; } = null!;
    }
}
