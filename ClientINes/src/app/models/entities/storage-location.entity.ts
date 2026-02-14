import { Item } from './item.entity';

export interface StorageLocation {
  id: string;
  name: string;
  color?: string;
  icon?: string;
  parentLocationId?: string | null;
  items: Item[];
  children?: StorageLocation[];
  showMenu?: boolean;
}