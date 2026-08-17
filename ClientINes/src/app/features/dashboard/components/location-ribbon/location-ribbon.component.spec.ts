import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LocationRibbonComponent } from './location-ribbon.component';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';

describe('LocationRibbonComponent', () => {
  let component: LocationRibbonComponent;
  let fixture: ComponentFixture<LocationRibbonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LocationRibbonComponent, TranslateModule.forRoot()],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(LocationRibbonComponent);
    component = fixture.componentInstance;
  });

  it('isLocActiveOnBoard должен возвращать true, если ID есть в activeBoardIds', () => {
    component.activeBoardIds = ['loc-1', 'loc-2'];
    expect(component.isLocActiveOnBoard('loc-1')).toBeTrue();
    expect(component.isLocActiveOnBoard('loc-3')).toBeFalse();
  });

  it('pagedLocations должен нарезать локации согласно текущей странице', () => {
    component.locations = [
      { id: '1' }, { id: '2' }, { id: '3' }, { id: '4' }, { id: '5' }
    ] as any;
    component.currentPage = 0;

    expect(component.pagedLocations.length).toBeLessThanOrEqual(component.dynamicPageSize);
  });
});