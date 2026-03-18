import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateCategoryDto } from '../../models/dtos/item.dto';
import { Category } from '../../models/entities/category.entity';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/categories`;

  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }

  create(dto: CreateCategoryDto): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, dto);
  }

  update(id: string, dto: CreateCategoryDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto);
  }

  rename(id: string, name: string): Observable<void> {
    return this.update(id, { name });
  }

  delete(id: string, targetCategoryId?: string | null): Observable<void> {
    let params = new HttpParams();
    
    if (targetCategoryId) {
      params = params.set('targetCategoryId', targetCategoryId);
    }

    return this.http.delete<void>(`${this.apiUrl}/${id}`, { params });
  }
}