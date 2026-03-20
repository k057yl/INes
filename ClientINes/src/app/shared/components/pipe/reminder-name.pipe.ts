import { Pipe, PipeTransform } from '@angular/core';
import { ReminderType } from '../../../models/enums/reminder-type.enum';

@Pipe({
  name: 'reminderName',
  standalone: true
})
export class ReminderNamePipe implements PipeTransform {
  transform(value: number | ReminderType): string {
    const names: Record<number, string> = {
      0: 'WARRANTY',
      1: 'MAINTENANCE',
      2: 'RETURN_ITEM',
      3: 'CUSTOM'
    };
    return names[value] || 'CUSTOM';
  }
}