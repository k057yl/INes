export interface LendItemDto {
  itemId: string;
  personName: string;
  expectedReturnDate?: string | null;
  comment?: string | null;
}

export interface ReturnItemDto {
  returnedDate?: string | null;
}