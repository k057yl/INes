import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PlatformService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/platforms`;

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  create(name: string): Observable<any> {
    return this.http.post<any>(this.apiUrl, JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  update(id: string, name: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}