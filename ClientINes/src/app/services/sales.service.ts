import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SellItemRequestDto, SaleResponseDto } from '../models/dtos/sale.dto';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SalesService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/sales`;

  sellItem(request: SellItemRequestDto): Observable<SaleResponseDto> {
    return this.http.post<SaleResponseDto>(this.apiUrl, request);
  }

  getHistory(): Observable<SaleResponseDto[]> {
    return this.http.get<SaleResponseDto[]>(this.apiUrl); 
  }
}