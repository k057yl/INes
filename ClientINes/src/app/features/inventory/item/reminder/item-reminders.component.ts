import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ReminderService } from '../../../../shared/services/reminder.service';
import { Reminder } from '../../../../models/entities/reminder.entity';
import { ReminderType } from '../../../../models/enums/reminder-type.enum';
import { TranslateModule } from '@ngx-translate/core';
import { InestModalComponent } from '../../../../shared/components/modal/shared-modal/inest-modal.component';

@Component({
  selector: 'app-item-reminders',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, InestModalComponent],
  templateUrl: './item-reminders.component.html',
  styleUrls: ['./item-reminders.component.scss']
})
export class ItemRemindersComponent implements OnInit {
  @Input({ required: true }) itemId!: string;

  private reminderService = inject(ReminderService);
  private fb = inject(FormBuilder);

  reminders: Reminder[] = [];
  isAdding = false;
  reminderForm!: FormGroup;

  modalConfirmText: string = 'COMMON.CONFIRM';
  modalCancelText: string = 'COMMON.CANCEL';

  // СОСТОЯНИЕ МОДАЛКИ ПОДТВЕРЖДЕНИЯ
  showConfirm = false;
  modalMode: 'input' | 'delete' | 'confirm' | 'smart-delete' = 'confirm';
  modalTitle = '';
  modalMessage = '';
  pendingAction: { type: 'complete' | 'delete', id: string } | null = null;

  ReminderType = ReminderType;
  
  availableTypes = [
    { value: ReminderType.Maintenance, label: 'REMINDERS.MAINTENANCE' },
    { value: ReminderType.Insurance, label: 'REMINDERS.INSURANCE' },
    { value: ReminderType.WarrantyExpiration, label: 'REMINDERS.WARRANTY' },
    { value: ReminderType.MedicalCheckup, label: 'REMINDERS.MEDICAL' },
    { value: ReminderType.Custom, label: 'REMINDERS.CUSTOM' }
  ];

  ngOnInit(): void {
    this.loadReminders();
    this.initForm();
  }

  getReminderInfo(type: number) {
    const info: Record<number, { icon: string, color: string, label: string }> = {
        [ReminderType.Maintenance]: { icon: 'fa-tools', color: 'var(--g-green)', label: 'REMINDERS.MAINTENANCE' },
        [ReminderType.Insurance]: { icon: 'fa-file-shield', color: 'var(--accent-color)', label: 'REMINDERS.INSURANCE' },
        [ReminderType.WarrantyExpiration]: { icon: 'fa-calendar-check', color: 'var(--g-blue)', label: 'REMINDERS.WARRANTY' },
        [ReminderType.MedicalCheckup]: { icon: 'fa-stethoscopes', color: 'var(--g-red)', label: 'REMINDERS.MEDICAL' },
        [ReminderType.ReturnItem]: { icon: 'fa-hand-holding-heart', color: 'var(--g-yellow)', label: 'REMINDERS.RETURN_ITEM' },
        [ReminderType.Custom]: { icon: 'fa-bell', color: 'var(--text-muted)', label: 'REMINDERS.CUSTOM' }
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
      type: [ReminderType.Maintenance, Validators.required],
      triggerAt: ['', Validators.required]
    });
  }

  toggleAdd() {
    this.isAdding = !this.isAdding;
    if (!this.isAdding) this.reminderForm.reset({ type: ReminderType.Maintenance });
  }

  onSubmit() {
    if (this.reminderForm.invalid) return;
    const dto = {
      itemId: this.itemId,
      type: Number(this.reminderForm.value.type),
      triggerAt: new Date(this.reminderForm.value.triggerAt).toISOString()
    };
    this.reminderService.createReminder(dto).subscribe(newReminder => {
      this.reminders.unshift(newReminder);
      this.toggleAdd();
    });
  }

  // --- ЛОГИКА ПОДТВЕРЖДЕНИЙ ---

  requestComplete(id: string) {
    this.modalMode = 'confirm';
    this.modalTitle = 'REMINDERS.MODAL.CONFIRM_ACTION';
    this.modalMessage = 'REMINDERS.MODAL.M_YOU_SURE_DONE';
    this.modalConfirmText = 'REMINDERS.MODAL.YES';
    this.modalCancelText = 'REMINDERS.MODAL.CANCEL';
    this.pendingAction = { type: 'complete', id };
    this.showConfirm = true;
    }

  requestDelete(id: string) {
    this.modalMode = 'delete';
    this.modalTitle = 'COMMON.DELETE';
    this.modalMessage = 'REMINDERS.MODAL.M_YOU_SURE_DELETE';
    this.modalConfirmText = 'REMINDERS.MODAL.DELETE';
    this.modalCancelText = 'REMINDERS.MODAL.CANCEL';
    this.pendingAction = { type: 'delete', id };
    this.showConfirm = true;
  }

  handleConfirm() {
    if (!this.pendingAction) return;

    if (this.pendingAction.type === 'complete') {
      this.reminderService.completeReminder(this.pendingAction.id).subscribe(() => {
        this.loadReminders();
        this.showConfirm = false;
      });
    } else {
      this.reminderService.deleteReminder(this.pendingAction.id).subscribe(() => {
        this.reminders = this.reminders.filter(r => r.id !== this.pendingAction?.id);
        this.showConfirm = false;
      });
    }
  }
}