import { TestBed } from '@angular/core/testing';
import { DashboardLocationService } from './dashboard-location.service';
import { LocationService } from '../../location/services/location.service';
import { DashboardTreeService } from './dashboard-tree.service';
import { of, throwError } from 'rxjs';
import { StorageLocation } from '../../location/contracts/storage-location';

describe('DashboardLocationService', () => {
  let service: DashboardLocationService;
  let locationApiSpy: jasmine.SpyObj<LocationService>;
  let treeServiceSpy: jasmine.SpyObj<DashboardTreeService>;

  beforeEach(() => {
    locationApiSpy = jasmine.createSpyObj('LocationService', ['getTree', 'delete', 'rename', 'move', 'reorder']);
    treeServiceSpy = jasmine.createSpyObj('DashboardTreeService', [
      'flattenLocations',
      'excludeLocation',
      'getParentId',
      'canMoveLocation'
    ]);

    TestBed.configureTestingModule({
      providers: [
        DashboardLocationService,
        { provide: LocationService, useValue: locationApiSpy },
        { provide: DashboardTreeService, useValue: treeServiceSpy }
      ]
    });

    service = TestBed.inject(DashboardLocationService);
  });

  it('move() должен откатывать локальные изменения, если API вернул ошибку', (done) => {
    const initialLocations: StorageLocation[] = [{ id: 'loc-1', name: 'Гараж' } as StorageLocation];
    service.locations = [...initialLocations];

    treeServiceSpy.getParentId.and.returnValue(null);
    treeServiceSpy.canMoveLocation.and.returnValue(true);
    locationApiSpy.move.and.returnValue(throwError(() => new Error('API Error')));

    service.move('loc-1', 'loc-2').subscribe({
      error: () => {
        expect(service.locations).toEqual(initialLocations);
        done();
      }
    });
  });
});