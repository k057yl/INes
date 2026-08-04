import { Category } from '../../category/contracts/category';
import { StorageLocation } from '../../location/contracts/storage-location';
import { ItemStatus } from '../enums/item-status.enum';
import { Lending } from '../../lending/contracts/lending';
import { Reminder } from '../../reminder/contracts/reminder';
import { ItemHistory } from './item-history';
import { ItemPhoto } from './item-photo';
import { Sale } from '../../sales/contracts/sale';
import { ItemDetails } from './item-details';

export interface Item {
  id: string;
  name: string;
  description?: string;
  status: ItemStatus;
  createdAt: string;
  photoUrl?: string;

  details?: ItemDetails; 

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