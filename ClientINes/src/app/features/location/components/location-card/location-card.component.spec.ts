import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LocationCardComponent } from './location-card.component';
import { DashboardFacade } from '../../../dashboard/components/dashboard/dashboard.facade';
import { DashboardTreeService } from '../../../dashboard/services/dashboard-tree.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { TranslateModule } from '@ngx-translate/core';
import { StorageLocation } from '../../contracts/storage-location';
import { provideRouter } from '@angular/router';

describe('LocationCardComponent', () => {
  let component: LocationCardComponent;
  let fixture: ComponentFixture<LocationCardComponent>;
  let treeServiceSpy: jasmine.SpyObj<DashboardTreeService>;

  const mockLocation = {
    id: 'loc-1',
    name: 'Шкаф',
    color: '#007bff',
    children: [],
    items: [],
    parentLocationId: 'parent-1'
  } as unknown as StorageLocation;

  beforeEach(async () => {
    treeServiceSpy = jasmine.createSpyObj('DashboardTreeService', ['getLocationLevel', 'getParentId', 'canMoveLocation']);
    treeServiceSpy.getLocationLevel.and.returnValue(0);

    const facadeSpy = jasmine.createSpyObj('DashboardFacade', ['loadData'], {
      locations: { flatLocations: [mockLocation], locations: [mockLocation] }
    });

    await TestBed.configureTestingModule({
      imports: [LocationCardComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: DashboardFacade, useValue: facadeSpy },
        { provide: DashboardTreeService, useValue: treeServiceSpy },
        { provide: DashboardModalService, useValue: jasmine.createSpyObj('DashboardModalService', ['openLocationForm', 'openItemForm']) }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LocationCardComponent);
    component = fixture.componentInstance;
    component.location = { ...mockLocation, children: [], items: [] };
    fixture.detectChanges();
  });

  it('isMaxLevelReached должен возвращать true на 3-м уровне вложенности', () => {
    treeServiceSpy.getLocationLevel.and.returnValue(3);

    expect(component.isMaxLevelReached).toBeTrue();
  });

  it('cardBackgroundStyle должен возвращать стандартный фоновый цвет для корневой локации', () => {
    (component.location as any).parentLocationId = null;
    treeServiceSpy.getLocationLevel.and.returnValue(0);

    expect(component.cardBackgroundStyle).toBe('var(--bg-card)');
  });
});