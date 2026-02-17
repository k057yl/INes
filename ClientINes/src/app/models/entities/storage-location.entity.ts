import { Item } from './item.entity';

export interface StorageLocation {
  id: string;
  name: string;
  color?: string;
  icon?: string;
  parentLocationId?: string | null;
  isSalesLocation: boolean;
  isLendingLocation: boolean;
  items: Item[];
  children?: StorageLocation[];
  showMenu?: boolean;
}