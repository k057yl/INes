import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ItemFormModalComponent } from './item-form-modal.component';
import { ItemService } from '../../services/item.service';
import { LocationService } from '../../../location/services/location.service';
import { CategoryService } from '../../../category/services/category.service';
import { LendingService } from '../../../lending/services/lending.service';
import { AuthService } from '../../../auth/services/auth.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

describe('ItemFormModalComponent', () => {
  let component: ItemFormModalComponent;
  let fixture: ComponentFixture<ItemFormModalComponent>;

  beforeEach(async () => {
    const itemApiSpy = jasmine.createSpyObj('ItemService', ['createWithPhoto', 'update']);
    const locationApiSpy = jasmine.createSpyObj('LocationService', ['getAll']);
    const categoryApiSpy = jasmine.createSpyObj('CategoryService', ['getAll', 'create']);
    const authSpy = jasmine.createSpyObj('AuthService', [], { user$: of(null) });

    locationApiSpy.getAll.and.returnValue(of([]));
    categoryApiSpy.getAll.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [ItemFormModalComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: ItemService, useValue: itemApiSpy },
        { provide: LocationService, useValue: locationApiSpy },
        { provide: CategoryService, useValue: categoryApiSpy },
        { provide: LendingService, useValue: {} },
        { provide: AuthService, useValue: authSpy },
        { provide: DashboardModalService, useValue: jasmine.createSpyObj('DashboardModalService', ['confirm', 'close']) },
        { provide: ToastrService, useValue: jasmine.createSpyObj('ToastrService', ['success', 'error']) }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ItemFormModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('форма должна быть невалидной по умолчанию', () => {
    expect(component.form.valid).toBeFalse();
  });

  it('выбор статуса "Одолжено" (1) должен делать обязательными имя человека и дату возврата', () => {
    component.form.patchValue({ status: 1 });

    const personControl = component.form.get('personName');
    const returnDateControl = component.form.get('expectedReturnDate');

    expect(personControl?.invalid).toBeTrue();
    expect(returnDateControl?.invalid).toBeTrue();

    personControl?.setValue('Олег');
    returnDateControl?.setValue('2026-09-01');

    expect(personControl?.valid).toBeTrue();
    expect(returnDateControl?.valid).toBeTrue();
  });

  it('purchaseDate не должна быть в будущем', () => {
    const dateControl = component.form.get('purchaseDate');

    dateControl?.setValue('2099-01-01');
    expect(dateControl?.errors?.['futureDate']).toBeTrue();

    dateControl?.setValue('2026-08-01');
    expect(dateControl?.errors).toBeNull();
  });
});