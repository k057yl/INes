export interface CreateItemDto {
  name: string;
  description?: string;
  categoryId: string;
  storageLocationId?: string;
  status: number;
  purchaseDate?: string;
  purchasePrice?: number;
  estimatedValue?: number;
}