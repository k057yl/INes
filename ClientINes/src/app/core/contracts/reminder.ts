import { ReminderType } from '../enums/reminder-type.enum';
import { Item } from './item';
import { ReminderRecurrence } from '../enums/reminder-recurrence.enum';

export interface Reminder {
  id: string;
  itemId: string;
  title: string;
  type: number;
  recurrence: number;
  triggerAt: string;
  isCompleted: boolean;
  sendNotification: boolean;
}