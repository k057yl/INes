import { ItemHistoryType } from '../enums/item-history-type.enum';

export interface ItemHistory {
  id: string;
  itemId: string;
  type: ItemHistoryType;
  oldValue?: string;
  newValue?: string;
  comment?: string;
  createdAt: string;
}