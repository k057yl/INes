export interface SaleListItem {
  saleId: string;
  itemId: string;
  itemName: string;
  salePrice: number;
  profit: number;
  soldDate: string;
  platformName?: string;
  categoryName?: string;
}