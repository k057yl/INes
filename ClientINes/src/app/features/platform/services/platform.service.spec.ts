import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PlatformService } from './platform.service';
import { environment } from '../../../../environments/environment';

describe('PlatformService', () => {
  let service: PlatformService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiBaseUrl}/platforms`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PlatformService]
    });

    service = TestBed.inject(PlatformService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('rename() должен перенаправлять вызов в update() c PUT-методом', () => {
    const id = 'p-1';
    const newName = 'OLX Новая';

    service.rename(id, newName).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/${id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ name: newName });
    req.flush(null);
  });

  it('delete() должен отправлять DELETE-запрос на соответствующий id', () => {
    const id = 'p-1';

    service.delete(id).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/${id}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});