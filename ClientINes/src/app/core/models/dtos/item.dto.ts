import { ItemStatus } from '../enums/item-status.enum';

export interface CreateCategoryDto {
  name: string;
}

export interface CreateItemDto {
  name: string;
  description?: string;
  categoryId: string;
  storageLocationId?: string;
  status: ItemStatus;
  purchaseDate?: string;
  purchasePrice?: number;
  estimatedValue?: number;
  currency: string;
  personName?: string;
  contactEmail?: string;
  expectedReturnDate?: string;
  sendNotification?: boolean;
  mainPhotoName?: string;
}

export interface UpdateItemDto extends Partial<CreateItemDto> {}