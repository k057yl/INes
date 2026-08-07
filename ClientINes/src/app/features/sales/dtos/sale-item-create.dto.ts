export interface SaleCreateDto {
  itemId: string;
  salePrice: number;
  currency?: string;
  platformFee?: number;
  soldDate: string;
  platformId?: string | null;
  comment?: string;
}