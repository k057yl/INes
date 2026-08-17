import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LocationService } from './location.service';
import { environment } from '../../../../environments/environment';

describe('LocationService', () => {
  let service: LocationService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiBaseUrl}/locations`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LocationService]
    });

    service = TestBed.inject(LocationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('delete() должен добавлять targetLocationId в query-параметры при наличии', () => {
    const id = 'loc-1';
    const targetId = 'loc-2';

    service.delete(id, targetId).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/${id}?targetLocationId=${targetId}`);
    expect(req.request.method).toBe('DELETE');
    expect(req.request.params.get('targetLocationId')).toBe(targetId);
    req.flush(null);
  });

  it('reorder() должен отправлять PUT-запрос', () => {
    const payload = { parentId: null, orderedIds: ['1', '2'] };

    service.reorder(payload).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/reorder`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush(null);
  });
});