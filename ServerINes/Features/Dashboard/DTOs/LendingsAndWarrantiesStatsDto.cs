namespace INest.Features.Dashboard.DTOs
{
    public class LendingsAndWarrantiesStatsDto
    {
        public int ExpiredLendingsCount { get; set; }
        public int ExpiringLendingsCount { get; set; }
        public int ExpiringWarrantiesCount { get; set; }
        public List<AttentionItemDto> Items { get; set; } = new();
    }
}
