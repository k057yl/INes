import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HeaderComponent } from './header.component';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { of, Subject } from 'rxjs';
import { signal } from '@angular/core';

import { AuthService } from '../../../features/auth/services/auth.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { ThemeService } from '../../../core/services/theme.service';
import { DashboardModalService } from '../../../features/dashboard/components/dashboard/dashboard.modal.service';
import { LocationService } from '../../../features/location/services/location.service';

describe('HeaderComponent', () => {
  let component: HeaderComponent;
  let fixture: ComponentFixture<HeaderComponent>;

  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let localizationServiceSpy: jasmine.SpyObj<LocalizationService>;
  let themeServiceSpy: jasmine.SpyObj<ThemeService>;
  let modalServiceSpy: jasmine.SpyObj<DashboardModalService>;
  let locationServiceSpy: jasmine.SpyObj<LocationService>;

  const refreshSubject = new Subject<void>();

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['logout'], {
      user$: of({ displayName: 'Roman', roles: ['User'] })
    });
    localizationServiceSpy = jasmine.createSpyObj('LocalizationService', ['setLanguage'], {
      currentLang: 'ru'
    });
    
    themeServiceSpy = jasmine.createSpyObj('ThemeService', ['toggleTheme', 'isDarkTheme']);
    themeServiceSpy.isDarkTheme.and.returnValue(true as any);

    modalServiceSpy = jasmine.createSpyObj('DashboardModalService', ['openItemForm', 'openLocationForm'], {
      refreshData$: refreshSubject.asObservable()
    });
    locationServiceSpy = jasmine.createSpyObj('LocationService', ['getTree']);

    locationServiceSpy.getTree.and.returnValue(of([{ id: 'loc-1', name: 'Гараж' } as any]));

    await TestBed.configureTestingModule({
      imports: [
        HeaderComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
        { provide: LocalizationService, useValue: localizationServiceSpy },
        { provide: ThemeService, useValue: themeServiceSpy },
        { provide: DashboardModalService, useValue: modalServiceSpy },
        { provide: LocationService, useValue: locationServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('должен создаваться', () => {
    expect(component).toBeTruthy();
  });

  it('isAdmin должен корректно определять роль администратора', () => {
    expect(component.isAdmin({ roles: ['user', 'admin'] })).toBeTrue();
    expect(component.isAdmin({ roles: ['inest_admin'] })).toBeTrue();
    expect(component.isAdmin({ roles: 'admin' })).toBeTrue();
    expect(component.isAdmin({ roles: ['user'] })).toBeFalse();
    expect(component.isAdmin(null)).toBeFalse();
  });

  it('ngOnInit должен проверять наличие локаций', () => {
    expect(locationServiceSpy.getTree).toHaveBeenCalled();
    expect(component.hasLocations()).toBeTrue();
  });

  it('changeLang должен менять язык и закрывать меню выбора языка', () => {
    component.isLangMenuOpen.set(true);

    component.changeLang('uk');

    expect(localizationServiceSpy.setLanguage).toHaveBeenCalledWith('uk');
    expect(component.isLangMenuOpen()).toBeFalse();
  });

  it('openCreateItem должен открывать форму предмета и закрывать меню', () => {
    component.isCreateMenuOpen.set(true);

    component.openCreateItem();

    expect(modalServiceSpy.openItemForm).toHaveBeenCalled();
    expect(component.isCreateMenuOpen()).toBeFalse();
  });

  it('toggleTheme должен вызывать переключение темы', () => {
    component.toggleTheme();
    expect(themeServiceSpy.toggleTheme).toHaveBeenCalled();
  });
});