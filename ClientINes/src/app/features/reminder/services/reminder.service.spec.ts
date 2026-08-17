import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ReminderService } from './reminder.service';
import { environment } from '../../../../environments/environment';
import { ReminderType } from '../enums/reminder-type.enum';
import { ReminderRecurrence } from '../enums/reminder-recurrence.enum';

describe('ReminderService', () => {
  let service: ReminderService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiBaseUrl}/reminders`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ReminderService]
    });

    service = TestBed.inject(ReminderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getActiveReminders() должен делать GET на /active', () => {
    service.getActiveReminders().subscribe();

    const req = httpMock.expectOne(`${apiUrl}/active`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('createReminder() должен извлекать data из ответа бэкенда', () => {
    const mockCreatedReminder = { id: 'rem-1', title: 'ТО авто' } as any;
    const dto = {
      itemId: 'item-1',
      title: 'ТО авто',
      type: ReminderType.Maintenance,
      recurrence: ReminderRecurrence.Yearly,
      triggerAt: '2026-09-01T00:00:00Z',
      sendNotification: true,
      sendTelegram: true
    };

    service.createReminder(dto).subscribe(res => {
      expect(res).toEqual(mockCreatedReminder);
    });

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    req.flush({ data: mockCreatedReminder, message: 'Created' });
  });

  it('completeReminder() должен делать PATCH на /complete', () => {
    service.completeReminder('rem-1').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/rem-1/complete`);
    expect(req.request.method).toBe('PATCH');
    req.flush(null);
  });
});