import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SalesListComponent } from './sales-list.component';
import { SalesService } from '../../services/sales.service';
import { CategoryService } from '../../../category/services/category.service';
import { LocationService } from '../../../location/services/location.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { AuthService } from '../../../auth/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { SaleListItem } from '../../contracts/sale-list-item';

describe('SalesListComponent', () => {
  let component: SalesListComponent;
  let fixture: ComponentFixture<SalesListComponent>;
  let salesServiceSpy: jasmine.SpyObj<SalesService>;
  let toastrSpy: jasmine.SpyObj<ToastrService>;

  beforeEach(async () => {
    salesServiceSpy = jasmine.createSpyObj('SalesService', ['getPlatforms', 'getHistory', 'cancelSale', 'deleteSale']);
    const categorySpy = jasmine.createSpyObj('CategoryService', ['getAll']);
    const locationSpy = jasmine.createSpyObj('LocationService', ['getAll']);
    const authSpy = jasmine.createSpyObj('AuthService', [], { user$: of(null) });
    toastrSpy = jasmine.createSpyObj('ToastrService', ['success', 'error']);

    salesServiceSpy.getPlatforms.and.returnValue(of([]));
    salesServiceSpy.getHistory.and.returnValue(of([]));
    categorySpy.getAll.and.returnValue(of([]));
    locationSpy.getAll.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [SalesListComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: SalesService, useValue: salesServiceSpy },
        { provide: CategoryService, useValue: categorySpy },
        { provide: LocationService, useValue: locationSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: ToastrService, useValue: toastrSpy },
        { provide: DashboardModalService, useValue: jasmine.createSpyObj('DashboardModalService', ['openConfirm']) }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SalesListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('должен правильно суммировать выручку и прибыль по валютам', () => {
    component.sales = [
      { salePrice: 100, profit: 20, currency: 'USD' },
      { salePrice: 200, profit: 50, currency: 'USD' },
      { salePrice: 1000, profit: 300, currency: 'UAH' }
    ] as SaleListItem[];

    (component as any).calculateCurrencyTotals();

    expect(component.revenueByCurrency['USD']).toBe(300);
    expect(component.profitByCurrency['USD']).toBe(70);
    expect(component.revenueByCurrency['UAH']).toBe(1000);
  });

  it('onConfirmUndo не должен отменять продажу, если не выбрана локация возврата', () => {
    component.selectedSale = { itemId: 'item-1' } as SaleListItem;
    component.selectedReturnLocationId = null;

    component.onConfirmUndo();

    expect(toastrSpy.error).toHaveBeenCalled();
    expect(salesServiceSpy.cancelSale).not.toHaveBeenCalled();
  });
});