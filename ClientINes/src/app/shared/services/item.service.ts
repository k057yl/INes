import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Item } from '../../models/entities/item.entity';
import { CreateItemDto } from '../../models/dtos/item.dto';

@Injectable({ providedIn: 'root' })
export class ItemService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/items`;

  getAll(filters?: any): Observable<Item[]> {
    const params = this.cleanParams(filters);
    return this.http.get<Item[]>(this.apiUrl, { params });
  }

  private cleanParams(obj: any) {
    const params: any = {};
    if (!obj) return params;
    
    Object.keys(obj).forEach(key => {
      if (obj[key] !== null && obj[key] !== undefined && obj[key] !== '') {
        params[key] = obj[key];
      }
    });
    return params;
  }

  getById(id: string): Observable<Item> {
    return this.http.get<Item>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateItemDto): Observable<Item> {
    return this.http.post<Item>(this.apiUrl, dto);
  }

  createWithPhoto(data: FormData): Observable<Item> {
    return this.http.post<Item>(this.apiUrl, data);
  }

  update(id: string, data: FormData): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, data);
  }

  move(id: string, targetLocationId: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/move`, { targetLocationId });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}