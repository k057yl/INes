import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SellModalComponent } from './sell-modal.component';
import { SalesService } from '../../services/sales.service';
import { AuthService } from '../../../auth/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { Item } from '../../../item/contracts/item';

describe('SellModalComponent', () => {
  let component: SellModalComponent;
  let fixture: ComponentFixture<SellModalComponent>;
  let salesServiceSpy: jasmine.SpyObj<SalesService>;
  let toastrSpy: jasmine.SpyObj<ToastrService>;

  const mockItem = {
    id: 'item-10',
    status: 0,
    details: { purchasePrice: 100, currency: 'USD' }
  } as unknown as Item;

  beforeEach(async () => {
    salesServiceSpy = jasmine.createSpyObj('SalesService', ['getPlatforms', 'addPlatform']);
    toastrSpy = jasmine.createSpyObj('ToastrService', ['warning', 'success', 'error']);
    const authSpy = jasmine.createSpyObj('AuthService', [], { user$: of(null) });

    salesServiceSpy.getPlatforms.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [SellModalComponent, ReactiveFormsModule, TranslateModule.forRoot()],
      providers: [
        { provide: SalesService, useValue: salesServiceSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: ToastrService, useValue: toastrSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SellModalComponent);
    component = fixture.componentInstance;
    component.item = { ...mockItem };
    fixture.detectChanges();
  });

  it('onSubmit должен блокировать продажу, если вещь одолжена (status === 1)', () => {
    component.item.status = 1;
    spyOn(component.confirm, 'emit');

    component.onSubmit();

    expect(toastrSpy.warning).toHaveBeenCalled();
    expect(component.confirm.emit).not.toHaveBeenCalled();
  });

  it('onPlatformConfirmed должен создавать платформу и сразу подставлять её в форму', () => {
    salesServiceSpy.addPlatform.and.returnValue(of({ id: 'p-new', name: 'eBay' }));
    salesServiceSpy.getPlatforms.and.returnValue(of([{ id: 'p-new', name: 'eBay' }]));

    component.onPlatformConfirmed('eBay');

    expect(salesServiceSpy.addPlatform).toHaveBeenCalledWith({ name: 'eBay' });
    expect(component.sellForm.get('platformId')?.value).toBe('p-new');
  });
});