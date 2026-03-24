import { Category } from './category.entity';
import { StorageLocation } from './storage-location.entity';
import { ItemStatus } from '../enums/item-status.enum';
import { Lending } from './lending.entity';
import { Reminder } from './reminder.entity';

export interface ItemPhoto {
  id: string;
  url: string;
  filePath: string;
  isMain: boolean;
  uploadedAt: string;
}

export interface ItemHistory {
  id: string;
  itemId: string;
  type: number;
  oldValue?: string;
  newValue?: string;
  createdAt: string;
}

export interface Sale {
  id: string;
  price: number;
  soldDate: string;
  platformName?: string;
  profit: number;
}

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
  categoryId: string;
  category?: Category;
  storageLocationId?: string;
  storageLocation?: StorageLocation;
  photos: ItemPhoto[];
  history: ItemHistory[];
  sale?: Sale;
  lending?: Lending;
  reminders: Reminder[];
  hasActiveReminders: boolean;
}