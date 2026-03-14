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

  reorder(payload: { parentId: string | null, orderedIds: string[] }): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/reorder`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}