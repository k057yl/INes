import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ApiService } from './api.service';
import { environment } from '../../../environments/environment';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ApiService]
    });

    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('get() должен отправлять GET-запрос с корректным URL', () => {
    service.get('/test-path').subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/test-path`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('post() должен отправлять POST-запрос с телом', () => {
    const body = { key: 'value' };
    service.post('/test-path', body).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/test-path`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush({});
  });
});