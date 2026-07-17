import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Reminder } from '../../../core/contracts/reminder';
import { ReminderType } from '../../../core/enums/reminder-type.enum';
import { ReminderRecurrence } from '../../../core/enums/reminder-recurrence.enum';

@Component({
  selector: 'app-reminder-card',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './reminder-card.component.html',
  styleUrls: ['./reminder-card.component.scss']
})
export class ReminderCardComponent {
  @Input({ required: true }) reminder!: Reminder;
  @Output() onComplete = new EventEmitter<string>();
  @Output() onDelete = new EventEmitter<string>();

  ReminderRecurrence = ReminderRecurrence;

  get info() {
    const types: Record<number, { icon: string, color: string }> = {
      [ReminderType.Custom]: { icon: 'fa-bell', color: 'var(--text-muted)' },
      [ReminderType.Warranty]: { icon: 'fa-shield-alt', color: 'var(--g-blue)' },
      [ReminderType.Maintenance]: { icon: 'fa-tools', color: 'var(--g-green)' },
      [ReminderType.ReturnItem]: { icon: 'fa-undo', color: 'var(--g-yellow)' },
      [ReminderType.Insurance]: { icon: 'fa-file-invoice-dollar', color: 'var(--accent-color)' },
      [ReminderType.Medical]: { icon: 'fa-heartbeat', color: 'var(--g-red)' },
      [ReminderType.Tax]: { icon: 'fa-coins', color: 'var(--g-yellow)' },
      [ReminderType.Subscription]: { icon: 'fa-calendar-alt', color: 'var(--g-blue)' }
    };
    return types[this.reminder.type] || types[ReminderType.Custom];
  }

  getRecurrenceLabel(recurrence: number): string {
    const labels: Record<number, string> = {
      [ReminderRecurrence.Daily]: 'RECURRENCE.DAILY',
      [ReminderRecurrence.Weekly]: 'RECURRENCE.WEEKLY',
      [ReminderRecurrence.Monthly]: 'RECURRENCE.MONTHLY',
      [ReminderRecurrence.Yearly]: 'RECURRENCE.YEARLY'
    };
    return labels[recurrence] || '';
  }
}