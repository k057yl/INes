import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReminderCardComponent } from './reminder-card.component';
import { TranslateModule } from '@ngx-translate/core';
import { ReminderType } from '../../enums/reminder-type.enum';
import { ReminderRecurrence } from '../../enums/reminder-recurrence.enum';
import { Reminder } from '../../contracts/reminder';

describe('ReminderCardComponent', () => {
  let component: ReminderCardComponent;
  let fixture: ComponentFixture<ReminderCardComponent>;

  const mockReminder = {
    id: 'rem-1',
    title: 'Гарантия на смарт',
    type: ReminderType.Warranty,
    recurrence: ReminderRecurrence.None,
    isCompleted: false
  } as unknown as Reminder;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReminderCardComponent, TranslateModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(ReminderCardComponent);
    component = fixture.componentInstance;
    component.reminder = { ...mockReminder };
    fixture.detectChanges();
  });

  it('info должен возвращать правильную иконку и цвет для типа Warranty', () => {
    expect(component.info.icon).toBe('fa-shield-alt');
    expect(component.info.color).toBe('var(--g-blue)');
  });

  it('getRecurrenceLabel должен возвращать ключи локализации', () => {
    expect(component.getRecurrenceLabel(ReminderRecurrence.Daily)).toBe('RECURRENCE.DAILY');
    expect(component.getRecurrenceLabel(ReminderRecurrence.Yearly)).toBe('RECURRENCE.YEARLY');
    expect(component.getRecurrenceLabel(ReminderRecurrence.None)).toBe('');
  });
});