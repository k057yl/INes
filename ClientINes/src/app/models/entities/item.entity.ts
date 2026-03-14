import { Category } from './category.entity';
import { StorageLocation } from './storage-location.entity';
import { ItemStatus } from '../enums/item-status.enum';

export interface ItemPhoto {
  id: string;
  url: string;
  filePath: string;
  isMain: boolean;
  uploadedAt: string;
}

export interface ItemHistory {
  id: string;
  changeDate: string;
  description: string;
}

export interface Sale { price: number; }
export interface Lending { borrowerName: string; }
export interface Reminder { id: string; title: string; date: string; }

export interface Item {
  id: string;
  name: string;
  description?: string;
  status: ItemStatus;
  purchaseDate?: string;
  purchasePrice?: number;
  estimatedValue?: number;
  createdAt: string;
  photoUrl?: string;
  
  // Связи
  categoryId: string;
  category?: Category;
  storageLocationId?: string;
  storageLocation?: StorageLocation;
  photos: ItemPhoto[];
  history: ItemHistory[];
  reminders: Reminder[];
  sale?: Sale;
  lending?: Lending;
}