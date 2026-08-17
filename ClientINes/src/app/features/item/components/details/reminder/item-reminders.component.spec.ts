import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ItemRemindersComponent } from './item-reminders.component';
import { ReminderService } from '../../../../reminder/services/reminder.service';
import { DashboardModalService } from '../../../../dashboard/components/dashboard/dashboard.modal.service';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { ReminderType } from '../../../../reminder/enums/reminder-type.enum';

describe('ItemRemindersComponent', () => {
  let component: ItemRemindersComponent;
  let fixture: ComponentFixture<ItemRemindersComponent>;
  let reminderServiceSpy: jasmine.SpyObj<ReminderService>;
  let modalServiceSpy: jasmine.SpyObj<DashboardModalService>;

  beforeEach(async () => {
    reminderServiceSpy = jasmine.createSpyObj('ReminderService', ['getItemReminders', 'createReminder', 'completeReminder', 'deleteReminder']);
    modalServiceSpy = jasmine.createSpyObj('DashboardModalService', ['openConfirm']);

    reminderServiceSpy.getItemReminders.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [ItemRemindersComponent, ReactiveFormsModule, TranslateModule.forRoot()],
      providers: [
        { provide: ReminderService, useValue: reminderServiceSpy },
        { provide: DashboardModalService, useValue: modalServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ItemRemindersComponent);
    component = fixture.componentInstance;
    component.itemId = 'item-100';
    fixture.detectChanges();
  });

  it('должен создаваться и подгружать напоминания при инициализации', () => {
    expect(component).toBeTruthy();
    expect(reminderServiceSpy.getItemReminders).toHaveBeenCalledWith('item-100');
  });

  it('onSubmit должен отправлять DTO создания напоминания при валидной форме', () => {
    reminderServiceSpy.createReminder.and.returnValue(of({ id: 'rem-1', title: 'Проверка' } as any));

    component.isAdding = true;
    component.reminderForm.patchValue({
      title: 'Замена батареек',
      type: ReminderType.Maintenance,
      recurrence: 0,
      triggerAt: '2026-09-01T12:00:00.000Z'
    });

    component.onSubmit();

    expect(reminderServiceSpy.createReminder).toHaveBeenCalled();
    expect(component.reminders.length).toBe(1);
    expect(component.isAdding).toBeFalse();
  });

  it('requestDelete должен открывать подтверждающую модалку и удалять напоминание', () => {
    modalServiceSpy.openConfirm.and.returnValue(of('delete'));
    reminderServiceSpy.deleteReminder.and.returnValue(of(void 0));
    component.reminders = [{ id: 'rem-1', title: 'Тест' } as any];

    component.requestDelete('rem-1');

    expect(modalServiceSpy.openConfirm).toHaveBeenCalled();
    expect(reminderServiceSpy.deleteReminder).toHaveBeenCalledWith('rem-1');
    expect(component.reminders.length).toBe(0);
  });
});