export interface ItemLendDto {
  itemId: string;
  personName: string;
  expectedReturnDate?: string | null;
  comment?: string | null;
  valueAtLending?: number;
}