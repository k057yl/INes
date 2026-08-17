import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ItemService } from './item.service';
import { environment } from '../../../../environments/environment';

describe('ItemService', () => {
  let service: ItemService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiBaseUrl}/items`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ItemService]
    });

    service = TestBed.inject(ItemService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getAll() должен отсекать null и пустые параметры фильтрации', () => {
    service.getAll({ searchQuery: 'Ноут', categoryId: null, minPrice: undefined }).subscribe();

    const req = httpMock.expectOne(`${apiUrl}?searchQuery=%D0%9D%D0%BE%D1%83%D1%82`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('categoryId')).toBeFalse();
    req.flush([]);
  });

  it('changeStatus() должен отправлять PATCH с Content-Type application/json', () => {
    const id = 'item-1';
    const status = 1;

    service.changeStatus(id, status).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/${id}/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.headers.get('Content-Type')).toBe('application/json');
    expect(req.request.body).toBe(status);
    req.flush(null);
  });

  it('deleteArchivedBatch() должен отправлять DELETE с массивом id в body', () => {
    const ids = ['1', '2'];

    service.deleteArchivedBatch(ids).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/archived/batch`);
    expect(req.request.method).toBe('DELETE');
    expect(req.request.body).toEqual(ids);
    req.flush(null);
  });
});