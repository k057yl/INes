import { CreateItemDto } from '../dtos/create-item.dto';

export interface Category {
  id: string;
  name: string;
  color: string;
}

export interface Item extends CreateItemDto {
  id: string;
  createdAt: string;
  photoUrl?: string;
  publicId?: string;
  photos: ItemPhoto[];
  category?: Category;
  locationId: string;     
  locationName?: string;
}

export interface ItemPhoto {
  id: string;
  filePath: string;
  isMain: boolean;
  uploadedAt: string;
}