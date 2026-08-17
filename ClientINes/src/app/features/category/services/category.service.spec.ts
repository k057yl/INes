import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CategoryService } from './category.service';
import { environment } from '../../../../environments/environment';

describe('CategoryService', () => {
  let service: CategoryService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiBaseUrl}/categories`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CategoryService]
    });

    service = TestBed.inject(CategoryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getAll() должен отправлять GET-запрос', () => {
    const mockCategories = [{ id: '1', name: 'Одежда' }] as any;

    service.getAll().subscribe(res => {
      expect(res).toEqual(mockCategories);
    });

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('GET');
    req.flush(mockCategories);
  });

  it('create() должен отправлять POST-запрос с DTO', () => {
    const dto = { name: 'Электроника' };
    const mockCreated = { id: '2', ...dto } as any;

    service.create(dto).subscribe(res => {
      expect(res).toEqual(mockCreated);
    });

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush(mockCreated);
  });

  it('delete() должен добавлять targetCategoryId в query-параметры', () => {
    const catId = 'cat-1';
    const targetCatId = 'cat-target';

    service.delete(catId, targetCatId).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/${catId}?targetCategoryId=${targetCatId}`);
    expect(req.request.method).toBe('DELETE');
    expect(req.request.params.get('targetCategoryId')).toBe(targetCatId);
    req.flush(null);
  });
});