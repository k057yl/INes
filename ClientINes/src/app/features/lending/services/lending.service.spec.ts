import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LendingService } from './lending.service';
import { environment } from '../../../../environments/environment';
import { ItemLendDto } from '../../item/dtos/item-lend.dto';
import { ItemReturnDto } from '../../item/dtos/item-return.dto';

describe('LendingService', () => {
  let service: LendingService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiBaseUrl}/lending`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LendingService]
    });

    service = TestBed.inject(LendingService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('lendItem() должен отправлять POST-запрос с DTO аренды', () => {
    const dto: ItemLendDto = {
      itemId: 'item-1',
      personName: 'Алексей',
      valueAtLending: 150,
      expectedReturnDate: '2026-09-01',
      comment: 'До сентября',
      contactEmail: null,
      sendNotification: false,
      direction: 0
    };

    service.lendItem(dto).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/lend`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush({});
  });

  it('returnItem() должен отправлять POST-запрос на возврат вещи', () => {
    const itemId = 'item-1';
    const dto: ItemReturnDto = { returnedDate: '2026-08-17T10:00:00Z' };

    service.returnItem(itemId, dto).subscribe(res => {
      expect(res).toBeTrue();
    });

    const req = httpMock.expectOne(`${apiUrl}/${itemId}/return`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush(true);
  });
});