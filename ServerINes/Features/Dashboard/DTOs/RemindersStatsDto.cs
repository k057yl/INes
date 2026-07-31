namespace INest.Features.Dashboard.DTOs
{
    public class RemindersStatsDto
    {
        public int ExpiredCount { get; set; }
        public int ActiveCount { get; set; }
        public List<AttentionItemDto> Items { get; set; } = new();
    }
}
