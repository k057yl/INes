import { CreateItemDto } from '../dtos/create-item.dto';

export interface Item extends CreateItemDto {
  id: string;
  createdAt: string;
  photos: ItemPhoto[];
}

export interface ItemPhoto {
  id: string;
  filePath: string;
  isMain: boolean;
  uploadedAt: string;
}