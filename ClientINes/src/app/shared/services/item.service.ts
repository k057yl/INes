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

  getAll(): Observable<Item[]> {
    return this.http.get<Item[]>(this.apiUrl);
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

  deleteItem(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  cancelSale(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/cancel-sale`, {});
  }
}