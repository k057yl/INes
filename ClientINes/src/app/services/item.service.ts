import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateItemDto } from '../models/dtos/create-item.dto';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ItemService {
  private http = inject(HttpClient);

  create(dto: CreateItemDto): Observable<any> {
    return this.http.post(`${environment.apiBaseUrl}/items`, dto);
  }

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiBaseUrl}/items`);
  }

  get(id: string): Observable<any> {
    return this.http.get(`${environment.apiBaseUrl}/items/${id}`);
  }

  update(id: string, dto: CreateItemDto): Observable<any> {
    return this.http.put(`${environment.apiBaseUrl}/items/${id}`, dto);
  }

  delete(id: string): Observable<any> {
    return this.http.delete(`${environment.apiBaseUrl}/items/${id}`);
  }
}