import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ItemDetailComponent } from './item-detail.component';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { ItemService } from '../../services/item.service';
import { LendingService } from '../../../lending/services/lending.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { environment } from '../../../../../environments/environment';
import { Item } from '../../contracts/item';

describe('ItemDetailComponent', () => {
  let component: ItemDetailComponent;
  let fixture: ComponentFixture<ItemDetailComponent>;
  let httpMock: HttpTestingController;
  let routerSpy: jasmine.SpyObj<Router>;
  let toastrSpy: jasmine.SpyObj<ToastrService>;
  let modalServiceSpy: jasmine.SpyObj<DashboardModalService>;

  beforeEach(async () => {
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    toastrSpy = jasmine.createSpyObj('ToastrService', ['warning']);
    modalServiceSpy = jasmine.createSpyObj('DashboardModalService', ['openItemForm', 'openConfirm']);

    await TestBed.configureTestingModule({
      imports: [ItemDetailComponent, HttpClientTestingModule, TranslateModule.forRoot()],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'item-123' } } }
        },
        { provide: Router, useValue: routerSpy },
        { provide: ToastrService, useValue: toastrSpy },
        { provide: DashboardModalService, useValue: modalServiceSpy },
        ItemService,
        LendingService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ItemDetailComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('ngOnInit должен подгружать предмет и сортировать историю по дате', () => {
    const mockItem = {
      id: 'item-123',
      name: 'Ноутбук',
      status: 0,
      history: [
        { id: '1', createdAt: '2026-08-17T10:00:00' },
        { id: '2', createdAt: '2026-08-10T10:00:00' }
      ]
    } as unknown as Item;

    fixture.detectChanges(); // Вызывает ngOnInit

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/items/item-123`);
    req.flush(mockItem);

    expect(component.item?.id).toBe('item-123');
    expect(component.item?.history[0].id).toBe('2'); // Более старая история в начале
  });

  it('onEdit должен запрещать редактирование неактивных вещей (status !== 0)', () => {
    component.item = { id: 'item-123', status: 1 } as Item; // status 1 = Lent

    component.onEdit();

    expect(toastrSpy.warning).toHaveBeenCalled();
    expect(modalServiceSpy.openItemForm).not.toHaveBeenCalled();
  });

  it('при 404 ошибке должен редиректить на /dashboard', () => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/items/item-123`);
    req.flush('Not Found', { status: 404, statusText: 'Not Found' });

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/dashboard'], { replaceUrl: true });
  });
});