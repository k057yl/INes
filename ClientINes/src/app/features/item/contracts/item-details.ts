export interface ItemDetails {
  purchaseDate?: string;
  purchasePrice?: number;
  estimatedValue?: number;
  currency: string;
  warrantyExpiration?: string;
  warrantyAlertSent: boolean;
  receiptDocumentPath?: string;
}