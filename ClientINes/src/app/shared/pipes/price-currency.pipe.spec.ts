import { TestBed } from '@angular/core/testing';
import { PricePipe } from './price-currency.pipe';
import { CurrencyPipe, registerLocaleData } from '@angular/common';
import { TranslateService } from '@ngx-translate/core';
import localeRu from '@angular/common/locales/ru';
import localeUk from '@angular/common/locales/uk';

registerLocaleData(localeRu, 'ru');
registerLocaleData(localeUk, 'uk');

describe('PricePipe', () => {
  let pipe: PricePipe;
  let translateServiceSpy: jasmine.SpyObj<TranslateService>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('TranslateService', [], {
      currentLang: 'ru',
      defaultLang: 'ru'
    });

    TestBed.configureTestingModule({
      providers: [
        PricePipe,
        CurrencyPipe,
        { provide: TranslateService, useValue: spy }
      ]
    });

    pipe = TestBed.inject(PricePipe);
    translateServiceSpy = TestBed.inject(TranslateService) as jasmine.SpyObj<TranslateService>;
  });

  it('должен создаваться', () => {
    expect(pipe).toBeTruthy();
  });

  it('должен возвращать прочерк "—" для null или undefined', () => {
    expect(pipe.transform(null)).toBe('—');
    expect(pipe.transform(undefined)).toBe('—');
  });

  it('должен форматировать USDT отдельным стандартом', () => {
    const result = pipe.transform(150, 'USDT');
    expect(result).toBe('150 USDT');
  });

  it('должен форматировать стандартную валюту (USD)', () => {
    const result = pipe.transform(100, 'USD');
    expect(result).toContain('100');
    expect(result).toContain('$');
  });
});