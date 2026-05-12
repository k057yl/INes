export interface SellItemRequestDto {
  itemId: string;
  salePrice: number;
  platformFee?: number;
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
  categoryName?: string;
}

export interface SaleFilters {
  searchQuery?: string | null;
  platformId?: string | null;
  categoryId?: string | null;
  sortBy?: number | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  minProfit?: number | null;
  startDate?: string | null;
  endDate?: string | null;
}