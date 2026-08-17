import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { SalesService } from './sales.service';
import { environment } from '../../../../environments/environment';

describe('SalesService', () => {
  let service: SalesService;
  let httpMock: HttpTestingController;
  const salesUrl = `${environment.apiBaseUrl}/sales`;
  const platformsUrl = `${environment.apiBaseUrl}/platforms`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [SalesService]
    });

    service = TestBed.inject(SalesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getHistory() должен собирать query-параметры фильтров', () => {
    service.getHistory({ platformId: 'plat-1', minPrice: 100 } as any).subscribe();

    const req = httpMock.expectOne(`${salesUrl}?platformId=plat-1&minPrice=100`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('cancelSale() должен отправлять DELETE с locationId в params', () => {
    const itemId = 'item-1';
    const locationId = 'loc-1';

    service.cancelSale(itemId, locationId).subscribe();

    const req = httpMock.expectOne(`${salesUrl}/cancel/${itemId}?locationId=${locationId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('addPlatform() должен отправлять POST-запрос', () => {
    const dto = { name: 'OLX' };

    service.addPlatform(dto).subscribe();

    const req = httpMock.expectOne(platformsUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush({ id: 'p-1', name: 'OLX' });
  });
});