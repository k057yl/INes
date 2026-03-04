import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
export class LocalizationService {

  constructor(private translate: TranslateService) {
    translate.addLangs(['en', 'ru', 'uk']);
    translate.setDefaultLang('en');

    const saved = localStorage.getItem('lang') || 'en';
    translate.use(saved);
  }

  get currentLang(): string {
    return this.translate.currentLang || 'en';
  }

  setLanguage(lang: string) {
    localStorage.setItem('lang', lang);
    this.translate.use(lang);
  }

  getLanguage(): string {
    return this.currentLang;
  }
}