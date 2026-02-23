import { CreateItemDto } from '../dtos/create-item.dto';

export interface Category {
  id: string;
  name: string;
  color: string;
}

// Добавляем недостающие интерфейсы для коллекций
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

// Интерфейсы-заглушки (наполни их полями из своих Sale/Lending сущностей в C#)
export interface Sale { price: number; }
export interface Lending { borrowerName: string; }
export interface Reminder { id: string; title: string; date: string; }

export interface Item extends CreateItemDto {
  id: string;
  createdAt: string;
  photoUrl?: string;
  publicId?: string;
  
  // Связи
  photos: ItemPhoto[];
  category?: Category;
  storageLocation?: any;
  history: ItemHistory[];
  reminders: Reminder[];
  sale?: Sale;
  lending?: Lending;
  locationId?: string;     
  locationName?: string;
}