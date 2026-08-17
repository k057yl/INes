import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DashboardComponent } from './dashboard.component';
import { DashboardFacade } from './dashboard.facade';
import { DashboardModalService } from './dashboard.modal.service';
import { TutorialService } from '../../../../core/services/tutorial.service';
import { AuthService } from '../../../auth/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { StorageLocation } from '../../../location/contracts/storage-location';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let facadeSpy: jasmine.SpyObj<DashboardFacade>;
  let modalSpy: jasmine.SpyObj<DashboardModalService>;

  beforeEach(async () => {
    const locationsSpy = jasmine.createSpyObj('Locations', ['delete'], {
      locations: [],
      flatLocations: []
    });
    locationsSpy.delete.and.returnValue(of(void 0));

    facadeSpy = jasmine.createSpyObj('DashboardFacade', ['loadData'], {
      locations: locationsSpy,
      nav: jasmine.createSpyObj('Nav', ['getBoardPageLocations', 'getTotalBoardPages', 'changeBoardPage']),
      executor: jasmine.createSpyObj('Executor', ['run']),
      items: jasmine.createSpyObj('Items', ['moveLocally', 'moveApi', 'delete']),
      tree: jasmine.createSpyObj('Tree', ['isChildOf', 'canMoveLocation'])
    });

    facadeSpy.loadData.and.returnValue(of({}));

    modalSpy = jasmine.createSpyObj('DashboardModalService', ['openConfirm', 'openNotEmptyLocationDelete', 'close'], {
      refreshData$: of()
    });

    const authSpy = jasmine.createSpyObj('AuthService', [], { user$: of(null) });
    const tutorialSpy = jasmine.createSpyObj('TutorialService', ['startDashboardTour']);

    await TestBed.configureTestingModule({
      imports: [DashboardComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: DashboardFacade, useValue: facadeSpy },
        { provide: DashboardModalService, useValue: modalSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: TutorialService, useValue: tutorialSpy }
      ]
    }).overrideComponent(DashboardComponent, {
      set: { providers: [] }
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('должен создаваться', () => {
    expect(component).toBeTruthy();
  });

  it('onDeleteLocation должен вызывать окно непустой локации, если в ней есть вещи', () => {
    const locWithItems = { id: 'loc-1', items: [{ id: 'item-1' }] } as unknown as StorageLocation;

    component.onDeleteLocation(locWithItems);

    expect(modalSpy.openNotEmptyLocationDelete).toHaveBeenCalled();
  });

  it('onDeleteLocation должен вызывать стандартное подтверждение, если локация пуста', () => {
    modalSpy.openConfirm.and.returnValue(of(true));
    const emptyLoc = { id: 'loc-1', items: [], children: [] } as unknown as StorageLocation;

    component.onDeleteLocation(emptyLoc);

    expect(modalSpy.openConfirm).toHaveBeenCalled();
  });
});