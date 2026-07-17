import { ItemStatus } from '../enums/item-status.enum';

export interface ItemCreateDto {
  name: string;
  description?: string;
  categoryId: string;
  storageLocationId?: string;
  status: ItemStatus;
  
  details: {
    purchaseDate?: string;
    purchasePrice?: number;
    estimatedValue?: number;
    currency: string;
    warrantyExpiration?: string;
  };

  personName?: string;
  contactEmail?: string;
  expectedReturnDate?: string;
  sendNotification?: boolean;
  mainPhotoName?: string;
}