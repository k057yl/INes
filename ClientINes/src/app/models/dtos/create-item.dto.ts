import { ItemStatus } from '../enums/item-status.enum';

export interface CreateItemDto {
  name: string;
  description?: string;
  categoryId: string;
  storageLocationId?: string;
  status: ItemStatus;
  purchaseDate?: string;
  purchasePrice?: number;
  estimatedValue?: number;
}