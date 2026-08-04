import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GetPlatformDto } from '../../platform/dtos/platforms-get.dto';
import { Platform } from '../../platform/contracts/platform';
import { GetSalesDto } from '../dtos/sales-get.dto';
import { SaleListItem } from '../contracts/sale-list-item';
import { SaleCreateDto } from '../dtos/sale-item-create.dto';

@Injectable({ providedIn: 'root' })
export class SalesService {
  private http = inject(HttpClient);
  private salesUrl = `${environment.apiBaseUrl}/sales`;
  private platformsUrl = `${environment.apiBaseUrl}/platforms`;

  sellItem(dto: SaleCreateDto): Observable<SaleListItem> {
    return this.http.post<SaleListItem>(this.salesUrl, dto);
  }

  getHistory(filters?: GetSalesDto): Observable<SaleListItem[]> {
    let params = new HttpParams();
    
    if (filters) {
      Object.keys(filters).forEach(key => {
        const value = (filters as any)[key];
        if (value !== null && value !== undefined && value !== '') {
          params = params.set(key, value.toString());
        }
      });
    }

    return this.http.get<SaleListItem[]>(this.salesUrl, { params });
  }

  cancelSale(itemId: string, locationId: string): Observable<void> {
    return this.http.delete<void>(`${this.salesUrl}/cancel/${itemId}`, {
      params: { locationId }
    });
  }

  deleteSale(saleId: string): Observable<void> {
    return this.http.delete<void>(`${this.salesUrl}/${saleId}`);
  }

  getPlatforms(): Observable<Platform[]> {
    return this.http.get<Platform[]>(this.platformsUrl);
  }

  addPlatform(dto: GetPlatformDto): Observable<Platform> {
    return this.http.post<Platform>(this.platformsUrl, dto);
  }
}