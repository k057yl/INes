import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { SellItemRequestDto, SaleResponseDto } from '../models/dtos/sale.dto';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SalesService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/sales`;

  sellItem(dto: SellItemRequestDto) {
    return this.http.post<SaleResponseDto>(this.apiUrl, dto);
  }

  getHistory() {
    return this.http.get<SaleResponseDto[]>(this.apiUrl);
  }

  getPlatforms() {
    return this.http.get<any[]>(`${environment.apiBaseUrl}/platforms`);
  }

  addPlatform(name: string) {
    return this.http.post<any>(`${environment.apiBaseUrl}/platforms`, JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  cancelSale(itemId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiBaseUrl}/sales/${itemId}`);
  }
}