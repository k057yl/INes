import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { StorageLocation } from '../../models/entities/storage-location.entity';
import { CreateLocationDto } from '../../models/dtos/location.dto';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/locations`;

  create(dto: CreateLocationDto): Observable<StorageLocation> {
    return this.http.post<StorageLocation>(this.apiUrl, dto);
  }

  getAll(): Observable<StorageLocation[]> {
    return this.http.get<StorageLocation[]>(this.apiUrl);
  }

  getTree(): Observable<StorageLocation[]> {
    return this.http.get<StorageLocation[]>(`${this.apiUrl}/tree`);
  }

  getById(id: string): Observable<StorageLocation> {
    return this.http.get<StorageLocation>(`${this.apiUrl}/${id}`);
  }

  move(id: string, newParentId: string | null) {
    return this.http.patch(`${environment.apiBaseUrl}/locations/${id}/move`, { newParentId });
  }

  rename(id: string, name: string) {
    return this.http.patch(`${environment.apiBaseUrl}/locations/${id}/rename`, { name });
  }

  reorder(payload: { parentId: string | null, orderedIds: string[] }): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/reorder`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}