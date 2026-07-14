import { ReminderType } from '../enums/reminder-type.enum';

export interface RemindCreateDto {
  itemId: string;
  type: ReminderType;
  triggerAt: string; 
}