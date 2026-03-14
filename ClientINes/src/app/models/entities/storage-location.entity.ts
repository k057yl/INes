import { Item } from './item.entity';

export interface StorageLocation {
  id: string;
  name: string;
  description?: string;
  color?: string;
  icon?: string;
  parentId?: string | null;
  sortOrder: number;
  
  // UI свойства
  isSalesLocation: boolean;
  isLendingLocation: boolean;
  items: Item[];
  children?: StorageLocation[];
  showMenu?: boolean;
}