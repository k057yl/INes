import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ItemLendDto } from '../dtos/item-lend.dto';
import { ItemReturnDto } from '../dtos/item-return.dto';

@Injectable({
  providedIn: 'root'
})
export class LendingService {
  private readonly apiUrl = `${environment.apiBaseUrl}/lending`;

  constructor(private http: HttpClient) {}

  lendItem(dto: ItemLendDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/lend`, dto);
  }

  returnItem(itemId: string, dto: ItemReturnDto): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/${itemId}/return`, dto);
  }
}