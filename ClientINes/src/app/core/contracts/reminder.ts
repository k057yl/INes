import { ReminderType } from '../enums/reminder-type.enum';
import { ReminderRecurrence } from '../enums/reminder-recurrence.enum';
import { Item } from './item';

export interface Reminder {
  id: string;
  itemId: string;
  title: string;
  type: ReminderType;
  recurrence: ReminderRecurrence;
  triggerAt: string;
  isCompleted: boolean;
  sendNotification: boolean;
  sendTelegram?: boolean;
  item?: Item;
}