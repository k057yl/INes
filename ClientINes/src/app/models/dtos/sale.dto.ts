export interface SellItemRequestDto {
  itemId: string;
  salePrice: number;
  soldDate: string;
  platformId?: string | null;
  comment?: string;
}

export interface SaleResponseDto {
  saleId: string;
  itemId: string;
  itemName: string;
  salePrice: number;
  profit: number;
  soldDate: string;
  platformName?: string;
}