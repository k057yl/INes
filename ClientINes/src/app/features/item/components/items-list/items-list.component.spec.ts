import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ItemsListComponent } from './items-list.component';
import { ItemService } from '../../services/item.service';
import { CategoryService } from '../../../category/services/category.service';
import { LocationService } from '../../../location/services/location.service';
import { AuthService } from '../../../auth/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { Item } from '../../contracts/item';

describe('ItemsListComponent', () => {
  let component: ItemsListComponent;
  let fixture: ComponentFixture<ItemsListComponent>;
  let toastrSpy: jasmine.SpyObj<ToastrService>;

  beforeEach(async () => {
    const itemApiSpy = jasmine.createSpyObj('ItemService', ['getAll', 'deleteBatch', 'deleteArchivedBatch']);
    const categoryApiSpy = jasmine.createSpyObj('CategoryService', ['getAll']);
    const locationApiSpy = jasmine.createSpyObj('LocationService', ['getAll']);
    const authSpy = jasmine.createSpyObj('AuthService', [], { user$: of(null) });
    toastrSpy = jasmine.createSpyObj('ToastrService', ['error', 'warning', 'success']);

    itemApiSpy.getAll.and.returnValue(of([]));
    categoryApiSpy.getAll.and.returnValue(of([]));
    locationApiSpy.getAll.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [ItemsListComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: ItemService, useValue: itemApiSpy },
        { provide: CategoryService, useValue: categoryApiSpy },
        { provide: LocationService, useValue: locationApiSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: ToastrService, useValue: toastrSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ItemsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('toggleAll должен выделять все предметы на странице', () => {
    component.items = [
      { id: '1' } as Item,
      { id: '2' } as Item
    ];

    const mockEvent = { target: { checked: true } } as unknown as Event;
    component.toggleAll(mockEvent);

    expect(component.selectedIds.size).toBe(2);
    expect(component.isAllSelected()).toBeTrue();
  });

  it('bulkDelete должен запрещать удаление, если среди выбранных есть проданные вещи (status == 2)', () => {
    component.items = [{ id: '1', status: 2 }] as Item[];
    component.selectedIds.add('1');

    component.bulkDelete();

    expect(toastrSpy.error).toHaveBeenCalled();
  });
});