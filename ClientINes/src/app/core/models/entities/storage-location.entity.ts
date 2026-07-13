import { Item } from './item.entity';

export interface StorageLocation {
  id: string;
  name: string;

  description?: string;

  color?: string;

  icon?: string;

  parentLocationId?: string | null;

  parentLocation?: StorageLocation;

  sortOrder: number;

  items: Item[];

  children: StorageLocation[];

  showMenu?: boolean;

  itemsCount?: number;

  isSalesLocation: boolean;

  isLendingLocation: boolean;
}