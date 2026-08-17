import { TestBed } from '@angular/core/testing';
import { LocalizationService } from './localization.service';
import { TranslateService } from '@ngx-translate/core';

describe('LocalizationService', () => {
  let service: LocalizationService;
  let translateSpy: jasmine.SpyObj<TranslateService>;

  beforeEach(() => {
    localStorage.clear();
    translateSpy = jasmine.createSpyObj('TranslateService', ['addLangs', 'setDefaultLang', 'use']);

    TestBed.configureTestingModule({
      providers: [
        LocalizationService,
        { provide: TranslateService, useValue: translateSpy }
      ]
    });

    service = TestBed.inject(LocalizationService);
  });

  it('setLanguage() должен сохранять язык в localStorage и передавать в TranslateService', () => {
    service.setLanguage('uk');

    expect(localStorage.getItem('lang')).toBe('uk');
    expect(translateSpy.use).toHaveBeenCalledWith('uk');
  });

  it('getDefaultCurrency() должен возвращать UAH для украинского и USD по умолчанию', () => {
    const langSpy = spyOnProperty(service, 'currentLang', 'get').and.returnValue('uk');
    expect(service.getDefaultCurrency()).toBe('UAH');

    langSpy.and.returnValue('en');
    expect(service.getDefaultCurrency()).toBe('USD');
  });
});