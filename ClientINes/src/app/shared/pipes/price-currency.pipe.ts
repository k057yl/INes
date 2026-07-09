import { Pipe, PipeTransform } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { TranslateService } from '@ngx-translate/core';

@Pipe({
  name: 'price',
  pure: false
})
export class PricePipe implements PipeTransform {
  constructor(
    private currencyPipe: CurrencyPipe,
    private translate: TranslateService
  ) {}

  transform(value: number | null | undefined, currencyCode: string = 'USD'): string {
    if (value === null || value === undefined) return '—';

    const lang = this.translate.currentLang || this.translate.defaultLang || 'ru';
    const locale = this.getLocale(lang);
    
    if (currencyCode?.toUpperCase() === 'USDT') {
      const formatted = new Intl.NumberFormat(locale, { 
        minimumFractionDigits: 0, 
        maximumFractionDigits: 2 
      }).format(value);
      return `${formatted} USDT`;
    }

    return this.currencyPipe.transform(
      value,
      currencyCode,
      'symbol-narrow',
      '1.0-2',
      locale
    ) || '—';
  }

  private getLocale(lang: string): string {
    switch (lang) {
      case 'uk': return 'uk';
      case 'ru': return 'ru';
      default: return 'en-US';
    }
  }
}