import { Item } from "./item.entity";

export interface Lending {
  id: string;
  itemId: string;
  personName: string;
  dateGiven: string;
  expectedReturnDate?: string;
  returnedDate?: string;
  valueAtLending?: number;
  comment?: string;
  item?: Item;
  contactEmail?: string;
  sendNotification: boolean;
  direction: number;
}