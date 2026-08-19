export interface GetSalesDto {
  searchQuery?: string;
  platformId?: string;
  categoryId?: string;
  sortBy?: number;
  minPrice?: number;
  maxPrice?: number;
  minProfit?: number;
  startDate?: string;
  endDate?: string;
  pageNumber?: number;
  pageSize?: number;
}