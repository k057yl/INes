import { ReminderType } from '../enums/reminder-type.enum';
import { Item } from './item.entity';
import { ReminderRecurrence } from '../enums/reminder-recurrence.enum';

export interface Reminder {
  id: string;
  itemId: string;
  title: string;
  type: ReminderType;
  recurrence: ReminderRecurrence;
  triggerAt: string;
  isCompleted: boolean;
  item?: Item;
}