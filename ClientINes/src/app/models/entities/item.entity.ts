import { Category } from './category.entity';
import { StorageLocation } from './storage-location.entity';
import { ItemStatus } from '../enums/item-status.enum';
import { ReminderType } from '../enums/reminder-type.enum';

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

export interface Lending {
  id: string;
  itemId: string;
  personName: string;
  dateGiven: string;
  expectedReturnDate?: string;
  returnedDate?: string;
  comment?: string;
}

export interface Reminder {
  id: string;
  itemId: string;
  type: ReminderType;
  triggerAt: string;
  isCompleted: boolean;
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