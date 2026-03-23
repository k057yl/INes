import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LendItemDto, ReturnItemDto } from '../../models/dtos/lending.dto';
import { Lending } from '../../models/entities/lending.entity';

@Injectable({
  providedIn: 'root'
})
export class LendingService {
  private readonly apiUrl = `${environment.apiBaseUrl}/lending`;

  constructor(private http: HttpClient) {}

  lendItem(dto: LendItemDto): Observable<Lending> {
    return this.http.post<Lending>(`${this.apiUrl}/lend`, dto);
  }

  returnItem(itemId: string, dto: ReturnItemDto): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/${itemId}/return`, dto);
  }
}