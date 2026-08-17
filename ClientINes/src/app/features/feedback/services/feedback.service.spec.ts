import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FeedbackService } from './feedback.service';
import { FeedbackType } from '../enums/feedback-type.enum';
import { environment } from '../../../../environments/environment';

describe('FeedbackService', () => {
  let service: FeedbackService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiBaseUrl}/feedback`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [FeedbackService]
    });

    service = TestBed.inject(FeedbackService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('sendFeedback() должен отправлять POST-запрос с DTO', () => {
    const dto = { type: FeedbackType.Bug, message: 'Сломалась кнопка' };
    const mockRes = { id: 'fb-123' };

    service.sendFeedback(dto).subscribe(res => {
      expect(res).toEqual(mockRes);
    });

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush(mockRes);
  });

  it('getAdminFeedbacks() должен правильно формировать query-параметры и игнорировать null', () => {
    service.getAdminFeedbacks(1, 10, true, FeedbackType.Idea).subscribe();

    const req = httpMock.expectOne(`${apiUrl}?page=1&pageSize=10&isProcessed=true&type=${FeedbackType.Idea}`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('isProcessed')).toBe('true');
    expect(req.request.params.get('type')).toBe(String(FeedbackType.Idea));
    req.flush({ items: [], totalCount: 0, page: 1, pageSize: 10 });
  });

  it('toggleProcessed() должен вызывать PATCH-запрос', () => {
    const id = 'fb-123';

    service.toggleProcessed(id).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/${id}/toggle-processed`);
    expect(req.request.method).toBe('PATCH');
    req.flush(null);
  });
});