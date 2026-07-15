import { ItemStatus } from '../enums/item-status.enum';

export const ITEM_STATUS_LABELS: Record<ItemStatus, string> = {
  [ItemStatus.Active]: 'STATUS.ACTIVE',
  [ItemStatus.Lent]: 'STATUS.LENT',
  [ItemStatus.Sold]: 'STATUS.SOLD',
  [ItemStatus.Archived]: 'STATUS.ARCHIVED'
};

export const ITEM_STATUS_OPTIONS = Object.entries(ITEM_STATUS_LABELS).map(([value, label]) => ({
  value: Number(value),
  label
}));