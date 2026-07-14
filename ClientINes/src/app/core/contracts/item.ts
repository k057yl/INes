import { Category } from './category';
import { StorageLocation } from './storage-location';
import { ItemStatus } from '../enums/item-status.enum';
import { Lending } from './lending';
import { Reminder } from './reminder';
import { ItemHistory } from './item-history';
import { ItemPhoto } from './item-photo';
import { Sale } from './sale';

export interface Item {
  id: string;
  name: string;
  description?: string;
  status: ItemStatus;
  purchaseDate?: string;
  purchasePrice?: number;
  estimatedValue?: number;
  currency: string;
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
  isLendingOverdue: boolean;
  hasOverdueReminders: boolean;
  categoryName?: string;
  storageLocationName?: string;
}