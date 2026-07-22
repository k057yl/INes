export interface DashboardStatsDto {
  totalItemsCount: number;
  totalLocationsCount: number;
  expiredRemindersCount: number;
  expiringWarrantiesCount: number;
  lentItemsCount: number;
  archivedAndSoldItemsCount: number;
  activeRemindersCount?: number;
}