import { ReminderType } from '../enums/reminder-type.enum';

export interface CreateReminderDto {
  itemId: string;
  type: ReminderType;
  triggerAt: string; 
}