import { ItemStatus } from '../enums/item-status.enum';

export const ITEM_STATUS_LABELS: Record<ItemStatus, string> = {
  [ItemStatus.Active]: 'STATUS.ACTIVE',
  [ItemStatus.Lent]: 'STATUS.LENT',
  [ItemStatus.Lost]: 'STATUS.LOST',
  [ItemStatus.Broken]: 'STATUS.BROKEN',
  [ItemStatus.Sold]: 'STATUS.SOLD',
  [ItemStatus.Gifted]: 'STATUS.GIFTED',
  [ItemStatus.Listed]: 'STATUS.LISTED',
  [ItemStatus.Borrowed]: 'STATUS.BORROWED',
};

export const ITEM_STATUS_OPTIONS = Object.entries(ITEM_STATUS_LABELS).map(([value, label]) => ({
  value: Number(value),
  label
}));