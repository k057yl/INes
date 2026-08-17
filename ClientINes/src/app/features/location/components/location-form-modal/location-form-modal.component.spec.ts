import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LocationFormModalComponent } from './location-form-modal.component';
import { LocationService } from '../../services/location.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { AuthService } from '../../../auth/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';

describe('LocationFormModalComponent', () => {
  let component: LocationFormModalComponent;
  let fixture: ComponentFixture<LocationFormModalComponent>;
  let locationServiceSpy: jasmine.SpyObj<LocationService>;
  let modalSpy: jasmine.SpyObj<DashboardModalService>;

  beforeEach(async () => {
    locationServiceSpy = jasmine.createSpyObj('LocationService', ['create', 'update']);
    modalSpy = jasmine.createSpyObj('DashboardModalService', ['confirm', 'close']);
    const authSpy = jasmine.createSpyObj('AuthService', [], { user$: of(null) });

    await TestBed.configureTestingModule({
      imports: [LocationFormModalComponent, ReactiveFormsModule, TranslateModule.forRoot()],
      providers: [
        { provide: LocationService, useValue: locationServiceSpy },
        { provide: DashboardModalService, useValue: modalSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: ToastrService, useValue: jasmine.createSpyObj('ToastrService', ['success', 'error']) }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LocationFormModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('onSubmit должен генерировать случайный цвет, если цвет не был выбран', () => {
    locationServiceSpy.create.and.returnValue(of({ id: 'loc-new', name: 'Кладовка' } as any));
    component.form.patchValue({ name: 'Кладовка', color: '' });

    component.onSubmit();

    expect(locationServiceSpy.create).toHaveBeenCalledWith(jasmine.objectContaining({
      name: 'Кладовка',
      color: jasmine.stringMatching(/^var\(--g-/)
    }));
    expect(modalSpy.confirm).toHaveBeenCalled();
  });
});