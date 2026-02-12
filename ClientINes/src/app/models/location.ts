export interface CreateLocationDto {
  name: string;
  description?: string;
  parentId?: string | null;
  icon?: string;
  color?: string;
  sortOrder: number;
}

export interface StorageLocation extends CreateLocationDto {
  id: string;
  createdAt: Date;
}