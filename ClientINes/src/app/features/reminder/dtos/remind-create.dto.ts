import { ReminderType } from '../enums/reminder-type.enum';
import { ReminderRecurrence } from '../enums/reminder-recurrence.enum';

export interface RemindCreateDto {
  itemId: string;
  title: string;
  type: ReminderType;
  recurrence: ReminderRecurrence;
  triggerAt: string;
  sendNotification: boolean;
  sendTelegram: boolean;
}