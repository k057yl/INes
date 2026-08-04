import { AttentionItemDto } from "../../../features/item/dtos/item-attention.dto";

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