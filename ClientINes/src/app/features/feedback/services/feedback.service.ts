import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateFeedbackDto } from '../dtos/feedback-create.dto';
import { RateFeedbackDto } from '../dtos/feedback-rate.dto';
import { environment } from '../../../../environments/environment';
import { FeedbackType } from '../enums/feedback-type.enum';
import { PagedFeedbackResult } from '../contracts/feedback';

@Injectable({
  providedIn: 'root'
})
export class FeedbackService {
  private readonly apiUrl = `${environment.apiBaseUrl}/feedback`;

  constructor(private http: HttpClient) {}

  sendFeedback(dto: CreateFeedbackDto): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, dto);
  }

  rateFeedback(id: string, dto: RateFeedbackDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/rate`, dto);
  }

  getAdminFeedbacks(
    page: number, 
    pageSize: number, 
    isProcessed: boolean | null, 
    type: FeedbackType | null
  ): Observable<PagedFeedbackResult> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (isProcessed !== null) params = params.set('isProcessed', isProcessed);
    if (type !== null) params = params.set('type', type);

    return this.http.get<PagedFeedbackResult>(this.apiUrl, { params });
  }

  toggleProcessed(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/toggle-processed`, {});
  }
}