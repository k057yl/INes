import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateReminderDto } from '../../models/dtos/reminder.dto';
import { Reminder } from '../../models/entities/reminder.entity';

@Injectable({
  providedIn: 'root'
})
export class ReminderService {
  private readonly apiUrl = `${environment.apiBaseUrl}/reminders`;

  constructor(private http: HttpClient) {}

  getActiveReminders(): Observable<Reminder[]> {
    return this.http.get<Reminder[]>(`${this.apiUrl}/active`);
  }

  getItemReminders(itemId: string): Observable<Reminder[]> {
    return this.http.get<Reminder[]>(`${this.apiUrl}/item/${itemId}`);
  }

  createReminder(dto: CreateReminderDto): Observable<Reminder> {
    return this.http.post<Reminder>(this.apiUrl, dto);
  }

  completeReminder(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/complete`, {});
  }

  deleteReminder(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}