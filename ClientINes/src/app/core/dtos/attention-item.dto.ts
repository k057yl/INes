export interface AttentionItemDto {
  itemId: string;
  itemName: string;
  locationName: string;
  typeKey: string;
  date: string | Date;
  severity: 'danger' | 'warning' | 'info';
}