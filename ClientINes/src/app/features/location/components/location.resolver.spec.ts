import { TestBed } from '@angular/core/testing';
import { locationResolver } from './location.resolver';
import { LocationService } from '../services/location.service';
import { Router, ActivatedRouteSnapshot } from '@angular/router';
import { of, throwError } from 'rxjs';

describe('locationResolver', () => {
  let locationServiceSpy: jasmine.SpyObj<LocationService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    locationServiceSpy = jasmine.createSpyObj('LocationService', [
      'getLocationHeader',
      'getLocationItems',
      'getLocationChildren'
    ]);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: LocationService, useValue: locationServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });
  });

  it('должен собирать данные локации из параллельных запросов', (done) => {
    const route = { paramMap: { get: () => 'loc-1' } } as unknown as ActivatedRouteSnapshot;

    locationServiceSpy.getLocationHeader.and.returnValue(of({ id: 'loc-1', name: 'Гараж' }));
    locationServiceSpy.getLocationItems.and.returnValue(of([{ id: 'i1' }]));
    locationServiceSpy.getLocationChildren.and.returnValue(of([]));

    TestBed.runInInjectionContext(() => {
      const result$ = locationResolver(route, {} as any) as any;
      result$.subscribe((data: any) => {
        expect(data.id).toBe('loc-1');
        expect(data.items.length).toBe(1);
        done();
      });
    });
  });

  it('должен редиректить на /dashboard при отсутствии ID в URL', () => {
    const route = { paramMap: { get: () => null } } as unknown as ActivatedRouteSnapshot;

    TestBed.runInInjectionContext(() => {
      locationResolver(route, {} as any);
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/dashboard']);
    });
  });
});