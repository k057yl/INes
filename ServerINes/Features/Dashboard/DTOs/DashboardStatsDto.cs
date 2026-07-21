namespace INest.Features.Dashboard.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalItemsCount { get; set; }
        public int TotalLocationsCount { get; set; }
        public int ExpiredRemindersCount { get; set; }
        public int ExpiringWarrantiesCount { get; set; }
        public int LentItemsCount { get; set; }
        public int ArchivedAndSoldItemsCount { get; set; }
    }
}
