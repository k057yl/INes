import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Observable } from 'rxjs';
import { StorageLocation } from '../contracts/storage-location';
import { LocationCreateDto } from '../dtos/location-create.dto';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/locations`;

  create(dto: LocationCreateDto): Observable<StorageLocation> {
    return this.http.post<StorageLocation>(this.apiUrl, dto);
  }

  getAll(): Observable<StorageLocation[]> {
    return this.http.get<StorageLocation[]>(this.apiUrl);
  }

  getTree(): Observable<StorageLocation[]> {
    return this.http.get<StorageLocation[]>(`${this.apiUrl}/tree`);
  }

  getLocationHeader(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}/header`);
  }

  getLocationItems(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${id}/items`);
  }

  getLocationChildren(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${id}/children`);
  }

  getQrCodeUrl(id: string): string {
    return `${this.apiUrl}/${id}/qr`;
  }

  move(id: string, newParentId: string | null) {
    return this.http.patch(`${this.apiUrl}/${id}/move`, { newParentId });
  }

  rename(id: string, name: string) {
    return this.http.patch(`${this.apiUrl}/${id}/rename`, { name });
  }

  reorder(payload: { parentId: string | null, orderedIds: string[] }): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/reorder`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}