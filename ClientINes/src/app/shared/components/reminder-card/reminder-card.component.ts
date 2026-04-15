import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Reminder } from '../../../models/entities/reminder.entity';
import { ReminderType } from '../../../models/enums/reminder-type.enum';

@Component({
  selector: 'app-reminder-card',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="reminder-card" [style.border-left-color]="info.color" [class.is-completed]="reminder.isCompleted">
      <div class="reminder-icon" [style.color]="info.color">
        <i class="fa" [ngClass]="info.icon"></i>
      </div>
      <div class="reminder-body">
        <h4>{{ reminder.title }}</h4>
        <small>{{ reminder.triggerAt | date:'dd.MM.yyyy HH:mm' }}</small>
      </div>
      <div class="reminder-actions">
        <button *ngIf="!reminder.isCompleted" (click)="onComplete.emit(reminder.id)" class="btn-check">
          <i class="fa fa-check"></i>
        </button>
        <button (click)="onDelete.emit(reminder.id)" class="btn-trash">
          <i class="fa fa-trash"></i>
        </button>
      </div>
    </div>
  `,
  styleUrls: ['./reminder-card.component.scss']
})
export class ReminderCardComponent {
  @Input({ required: true }) reminder!: Reminder;
  @Output() onComplete = new EventEmitter<string>();
  @Output() onDelete = new EventEmitter<string>();

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
}