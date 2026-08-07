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
    currency: string;
    warrantyExpiration?: string;
    receiptDocumentPath?: string;
    receiptFile?: File | null;
  };

  personName?: string;
  contactEmail?: string;
  expectedReturnDate?: string;
  sendNotification?: boolean;
  mainPhotoName?: string;
}