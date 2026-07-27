import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateFeedbackDto } from '../dtos/create-feedback.dto';
import { RateFeedbackDto } from '../dtos/rate-feedback.dto';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FeedbackService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/feedback`;

  constructor(private http: HttpClient) {}

  sendFeedback(dto: CreateFeedbackDto): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, dto);
  }

  rateFeedback(id: string, dto: RateFeedbackDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/rate`, dto);
  }
}