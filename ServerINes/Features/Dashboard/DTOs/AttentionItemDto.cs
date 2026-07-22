namespace INest.Features.Dashboard.DTOs
{
    public class AttentionItemDto
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string TypeKey { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Severity { get; set; } = "info";
    }
}
