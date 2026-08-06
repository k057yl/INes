export interface ItemDetails {
  purchaseDate?: string;
  purchasePrice?: number;
  currency: string;
  warrantyExpiration?: string;
  warrantyAlertSent: boolean;
  receiptDocumentPath?: string;
}