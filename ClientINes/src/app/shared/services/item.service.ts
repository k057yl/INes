import { inject, Injectable } from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Item } from '../../models/entities/item.entity';
import { CreateItemDto } from '../../models/dtos/item.dto';
import { ItemFilters } from '../../models/entities/item.entity';

@Injectable({
  providedIn: 'root'
})
export class ItemService {

  private http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiBaseUrl}/items`;

  getAll(
    filters?: ItemFilters
  ): Observable<Item[]> {

    const params =
      this.buildParams(filters);

    return this.http.get<Item[]>(
      this.apiUrl,
      { params }
    );
  }

  private buildParams(
    obj?: ItemFilters
  ): HttpParams {

    let params = new HttpParams();

    if (!obj) {
      return params;
    }

    Object.keys(obj).forEach(key => {

      const value =
        obj[key as keyof ItemFilters];

      if (
        value !== null &&
        value !== undefined
      ) {

        const stringValue =
          value.toString().trim();

        if (stringValue !== '') {

          params = params.set(
            key,
            stringValue
          );
        }
      }
    });

    return params;
  }

  getById(id: string): Observable<Item> {

    return this.http.get<Item>(
      `${this.apiUrl}/${id}`
    );
  }

  create(dto: CreateItemDto): Observable<Item> {

    return this.http.post<Item>(
      this.apiUrl,
      dto
    );
  }

  createWithPhoto(
    data: FormData
  ): Observable<Item> {

    return this.http.post<Item>(
      this.apiUrl,
      data
    );
  }

  update(
    id: string,
    data: FormData
  ): Observable<void> {

    return this.http.patch<void>(
      `${this.apiUrl}/${id}`,
      data
    );
  }

  changeStatus(
    id: string,
    status: number
  ): Observable<void> {

    const headers = {
      'Content-Type': 'application/json'
    };

    return this.http.patch<void>(
      `${this.apiUrl}/${id}/status`,
      status,
      { headers }
    );
  }

  move(
    id: string,
    targetLocationId: string
  ): Observable<void> {

    return this.http.patch<void>(
      `${this.apiUrl}/${id}/move`,
      { targetLocationId }
    );
  }

  delete(id: string): Observable<void> {

    return this.http.delete<void>(
      `${this.apiUrl}/${id}`
    );
  }

  deleteBatch(
    ids: string[]
  ): Observable<void> {

    return this.http.delete<void>(
      `${this.apiUrl}/batch`,
      { body: ids }
    );
  }
}