import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LocationDetailComponent } from './location-detail.component';
import { ActivatedRoute, Router } from '@angular/router';
import { LocationService } from '../../services/location.service';
import { ItemService } from '../../../item/services/item.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';

describe('LocationDetailComponent', () => {
  let component: LocationDetailComponent;
  let fixture: ComponentFixture<LocationDetailComponent>;
  let itemServiceSpy: jasmine.SpyObj<ItemService>;
  let modalSpy: jasmine.SpyObj<DashboardModalService>;

  const mockResolvedLocation = {
    id: 'loc-1',
    name: 'Гараж',
    items: [{ id: 'item-1', name: 'Дрель' }],
    parentLocation: null
  };

  beforeEach(async () => {
    itemServiceSpy = jasmine.createSpyObj('ItemService', ['archive']);
    modalSpy = jasmine.createSpyObj('DashboardModalService', ['openConfirm', 'openItemForm']);

    await TestBed.configureTestingModule({
      imports: [LocationDetailComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { data: of({ locationData: mockResolvedLocation }) }
        },
        { provide: Router, useValue: jasmine.createSpyObj('Router', ['navigate']) },
        { provide: ItemService, useValue: itemServiceSpy },
        { provide: LocationService, useValue: jasmine.createSpyObj('LocationService', ['getQrCodeUrl']) },
        { provide: DashboardModalService, useValue: modalSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LocationDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('должен подгружать данные локации из ActivatedRoute data', () => {
    expect(component.location).toEqual(mockResolvedLocation);
    expect(component.isLoading).toBeFalse();
    expect(component.breadcrumbs.length).toBe(1);
  });

  it('onDeleteItem должен архивировать предмет и убирать его из локального списка', () => {
    modalSpy.openConfirm.and.returnValue(of(true));
    itemServiceSpy.archive.and.returnValue(of(void 0));

    component.onDeleteItem({ id: 'item-1' });

    expect(modalSpy.openConfirm).toHaveBeenCalled();
    expect(itemServiceSpy.archive).toHaveBeenCalledWith('item-1');
    expect(component.location.items.length).toBe(0);
  });
});