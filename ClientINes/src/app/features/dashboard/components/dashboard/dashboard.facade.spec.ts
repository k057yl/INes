import { TestBed } from '@angular/core/testing';
import { DashboardFacade } from './dashboard.facade';
import { DashboardLocationService } from '../../services/dashboard-location.service';
import { DashboardItemService } from '../../services/dashboard-item.service';
import { DashboardNavigationService } from '../../services/dashboard-navigation.service';
import { DashboardActionExecutor } from '../../services/dashboard-action-executor.service';
import { DashboardTreeService } from '../../services/dashboard-tree.service';
import { DashboardService } from '../../services/dashboard.service';
import { LocationService } from '../../../location/services/location.service';
import { provideToastr } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';
import { StorageLocation } from '../../../location/contracts/storage-location';

describe('DashboardFacade', () => {
  let facade: DashboardFacade;
  let locationApiSpy: jasmine.SpyObj<LocationService>;
  let dashboardServiceSpy: jasmine.SpyObj<DashboardService>;

  beforeEach(() => {
    locationApiSpy = jasmine.createSpyObj('LocationService', ['getTree']);
    dashboardServiceSpy = jasmine.createSpyObj('DashboardService', ['getStats']);

    TestBed.configureTestingModule({
      imports: [TranslateModule.forRoot()],
      providers: [
        DashboardFacade,
        DashboardLocationService,
        DashboardItemService,
        DashboardNavigationService,
        DashboardActionExecutor,
        DashboardTreeService,
        provideToastr(),
        { provide: LocationService, useValue: locationApiSpy },
        { provide: DashboardService, useValue: dashboardServiceSpy }
      ]
    });

    facade = TestBed.inject(DashboardFacade);
  });

  it('loadData() должен параллельно подгружать дерево локаций и статистику', (done) => {
    const mockTree = [{ id: 'loc-1', name: 'Гараж' }] as StorageLocation[];
    const mockStats = { totalItems: 5 } as any;

    locationApiSpy.getTree.and.returnValue(of(mockTree));
    dashboardServiceSpy.getStats.and.returnValue(of(mockStats));

    facade.loadData().subscribe(() => {
      expect(facade.stats).toEqual(mockStats);
      expect(facade.locations.locations).toEqual(mockTree);
      expect(facade.isLoading).toBeFalse();
      done();
    });
  });
});