import { ReminderType } from '../enums/reminder-type.enum';
import { Item } from './item.entity';

export interface Reminder {
  id: string;
  itemId: string;
  type: ReminderType;
  triggerAt: string;
  isCompleted: boolean;
  item?: Item;
}