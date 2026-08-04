import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { TelegramStatusContract } from '../contracts/telegram-status';

@Injectable({
  providedIn: 'root'
})
export class TelegramBotService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/telegram`;

  getStatus(): Observable<TelegramStatusContract> {
    return this.http.get<TelegramStatusContract>(`${this.apiUrl}/status`);
  }

  generateToken(): Observable<TelegramStatusContract> {
    return this.http.post<TelegramStatusContract>(`${this.apiUrl}/generate-token`, {});
  }

  unlink(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/unlink`, {});
  }
}