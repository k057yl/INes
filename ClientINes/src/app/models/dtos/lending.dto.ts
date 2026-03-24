export interface LendItemDto {
  itemId: string;
  personName: string;
  expectedReturnDate?: string | null;
  comment?: string | null;
  valueAtLending?: number;
}

export interface ReturnItemDto {
  returnedDate?: string | null;
}