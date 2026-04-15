import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ReminderService } from '../../../../shared/services/reminder.service';
import { Reminder } from '../../../../models/entities/reminder.entity';
import { ReminderType } from '../../../../models/enums/reminder-type.enum';
import { ReminderRecurrence } from '../../../../models/enums/reminder-recurrence.enum';
import { TranslateModule } from '@ngx-translate/core';
import { DashboardModalService } from '../../../dashboard/dashboard.modal.service';

@Component({
  selector: 'app-item-reminders',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './item-reminders.component.html',
  styleUrls: ['./item-reminders.component.scss']
})
export class ItemRemindersComponent implements OnInit {
  @Input({ required: true }) itemId!: string;

  private reminderService = inject(ReminderService);
  private fb = inject(FormBuilder);
  private modal = inject(DashboardModalService);

  reminders: Reminder[] = [];
  isAdding = false;
  reminderForm!: FormGroup;

  ReminderType = ReminderType;
  ReminderRecurrence = ReminderRecurrence;

  // Список типов для селекта (8 штук)
  availableTypes = [
    { value: ReminderType.Custom, label: 'REMINDERS.CUSTOM' },
    { value: ReminderType.Warranty, label: 'REMINDERS.WARRANTY' },
    { value: ReminderType.Maintenance, label: 'REMINDERS.MAINTENANCE' },
    { value: ReminderType.ReturnItem, label: 'REMINDERS.RETURN_ITEM' },
    { value: ReminderType.Insurance, label: 'REMINDERS.INSURANCE' },
    { value: ReminderType.Medical, label: 'REMINDERS.MEDICAL' },
    { value: ReminderType.Tax, label: 'REMINDERS.TAX' },
    { value: ReminderType.Subscription, label: 'REMINDERS.SUBSCRIPTION' }
  ];

  // Список повторений для селекта
  availableRecurrences = [
    { value: ReminderRecurrence.None, label: 'RECURRENCE.NONE' },
    { value: ReminderRecurrence.Daily, label: 'RECURRENCE.DAILY' },
    { value: ReminderRecurrence.Weekly, label: 'RECURRENCE.WEEKLY' },
    { value: ReminderRecurrence.Monthly, label: 'RECURRENCE.MONTHLY' },
    { value: ReminderRecurrence.Yearly, label: 'RECURRENCE.YEARLY' }
  ];

  ngOnInit(): void {
    this.loadReminders();
    this.initForm();
  }

  getReminderInfo(type: number) {
    const info: Record<number, { icon: string, color: string, label: string }> = {
        [ReminderType.Custom]: { icon: 'fa-bell', color: 'var(--text-muted)', label: 'REMINDERS.CUSTOM' },
        [ReminderType.Warranty]: { icon: 'fa-shield-alt', color: 'var(--g-blue)', label: 'REMINDERS.WARRANTY' },
        [ReminderType.Maintenance]: { icon: 'fa-tools', color: 'var(--g-green)', label: 'REMINDERS.MAINTENANCE' },
        [ReminderType.ReturnItem]: { icon: 'fa-undo', color: 'var(--g-yellow)', label: 'REMINDERS.RETURN_ITEM' },
        [ReminderType.Insurance]: { icon: 'fa-file-invoice-dollar', color: 'var(--accent-color)', label: 'REMINDERS.INSURANCE' },
        [ReminderType.Medical]: { icon: 'fa-heartbeat', color: 'var(--g-red)', label: 'REMINDERS.MEDICAL' },
        [ReminderType.Tax]: { icon: 'fa-coins', color: 'var(--g-yellow)', label: 'REMINDERS.TAX' },
        [ReminderType.Subscription]: { icon: 'fa-calendar-alt', color: 'var(--g-blue)', label: 'REMINDERS.SUBSCRIPTION' }
    };
    return info[type] || info[ReminderType.Custom];
  }

  loadReminders() {
    this.reminderService.getItemReminders(this.itemId).subscribe(res => {
      this.reminders = res;
    });
  }

  initForm() {
    this.reminderForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      type: [ReminderType.Custom, Validators.required],
      recurrence: [ReminderRecurrence.None, Validators.required],
      triggerAt: ['', Validators.required]
    });
  }

  toggleAdd() {
    this.isAdding = !this.isAdding;
    if (!this.isAdding) this.reminderForm.reset({ type: ReminderType.Custom, recurrence: ReminderRecurrence.None });
  }

  onSubmit() {
    if (this.reminderForm.invalid) return;
    const dto = {
      itemId: this.itemId,
      title: this.reminderForm.value.title,
      type: Number(this.reminderForm.value.type),
      recurrence: Number(this.reminderForm.value.recurrence),
      triggerAt: new Date(this.reminderForm.value.triggerAt).toISOString()
    };
    this.reminderService.createReminder(dto).subscribe(newReminder => {
      this.reminders.unshift(newReminder);
      this.toggleAdd();
    });
  }

  // --- ЛОГИКА ЧЕРЕЗ ГЛОБАЛЬНЫЙ СЕРВИС ---

  requestComplete(id: string) {
    this.modal.openConfirm({
      mode: 'confirm',
      title: 'REMINDERS.MODAL.TITLE_DONE',
      message: 'REMINDERS.MODAL.M_YOU_SURE_DONE',
      confirmText: 'COMMON.YES',
      cancelText: 'COMMON.NO'
    }).subscribe((res: any) => {
      if (res) {
        this.reminderService.completeReminder(id).subscribe(() => this.loadReminders());
      }
    });
  }

  requestDelete(id: string) {
    this.modal.openConfirm({
      mode: 'delete',
      title: 'COMMON.DELETE',
      message: 'REMINDERS.MODAL.DELETE_CONFIRM_MSG'
    }).subscribe((res: any) => {
      if (res) {
        this.reminderService.deleteReminder(id).subscribe(() => {
          this.reminders = this.reminders.filter(r => r.id !== id);
        });
      }
    });
  }
}