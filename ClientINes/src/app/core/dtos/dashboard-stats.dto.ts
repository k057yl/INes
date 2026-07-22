import { AttentionItemDto } from "./attention-item.dto";

export interface DashboardStatsDto {
  totalItemsCount: number;
  totalLocationsCount: number;
  expiredRemindersCount: number;
  expiringWarrantiesCount: number;
  lentItemsCount: number;
  soldItemsCount: number;
  activeRemindersCount?: number;
  attentionItems?: AttentionItemDto[];
}