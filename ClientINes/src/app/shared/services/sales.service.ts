import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { SellItemRequestDto, SaleResponseDto } from '../../models/dtos/sale.dto';
import { PlatformDto } from '../../models/dtos/platform.dto';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SalesService {
  private http = inject(HttpClient);
  private salesUrl = `${environment.apiBaseUrl}/sales`;
  private platformsUrl = `${environment.apiBaseUrl}/platforms`;

  sellItem(dto: SellItemRequestDto): Observable<SaleResponseDto> {
    return this.http.post<SaleResponseDto>(this.salesUrl, dto);
  }

  getHistory(): Observable<SaleResponseDto[]> {
    return this.http.get<SaleResponseDto[]>(this.salesUrl);
  }

  cancelSale(itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.salesUrl}/cancel/${itemId}`);
  }

  smartDelete(saleId: string, keepHistory: boolean): Observable<void> {
    return this.http.delete<void>(`${this.salesUrl}/smart-delete/${saleId}`, {
      params: { keepHistory: keepHistory.toString() }
    });
  }

  getPlatforms(): Observable<any[]> {
    return this.http.get<any[]>(this.platformsUrl);
  }

  addPlatform(name: string): Observable<any> {
    const dto: PlatformDto = { name };
    return this.http.post<any>(this.platformsUrl, dto);
  }
}