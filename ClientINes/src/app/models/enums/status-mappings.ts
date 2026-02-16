import { ItemStatus } from './item-status.enum';

export const ITEM_STATUS_LABELS: Record<ItemStatus, string> = {
  [ItemStatus.Active]: 'STATUS.ACTIVE',
  [ItemStatus.Lent]: 'STATUS.LENT',
  [ItemStatus.Lost]: 'STATUS.LOST',
  [ItemStatus.Broken]: 'STATUS.BROKEN',
  [ItemStatus.Sold]: 'STATUS.SOLD',
  [ItemStatus.Gifted]: 'STATUS.GIFTED',
  [ItemStatus.Listed]: 'STATUS.LISTED'
};