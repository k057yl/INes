import { Item } from './item.entity';

export interface StorageLocation {
  id: string;
  name: string;
  description?: string;
  color?: string;
  icon?: string;
  parentLocation?: StorageLocation;
  sortOrder: number;
  
  // UI свойства
  items: Item[];
  children?: StorageLocation[];
  showMenu?: boolean;
}