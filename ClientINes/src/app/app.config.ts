import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection, isDevMode, provideAppInitializer, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { provideServiceWorker } from '@angular/service-worker';
import { routes } from './app.routes';

import { jwtInterceptor, cultureInterceptor, globalErrorInterceptor } from './core/interceptors/app.interceptors';
import { AuthService } from './core/services/auth.service';

import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { CurrencyPipe } from '@angular/common';

import { LOCALE_ID, APP_INITIALIZER } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeRu from '@angular/common/locales/ru';

registerLocaleData(localeRu, 'ru');

export function initializeApp(authService: AuthService) {
  return () => authService.checkAuth();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimations(),

    provideHttpClient(
      withInterceptors([cultureInterceptor, jwtInterceptor, globalErrorInterceptor])
    ),

    { provide: LOCALE_ID, useValue: 'ru' },

    provideAppInitializer(() => {
      const authService = inject(AuthService);
      return authService.checkAuth();
    }),

    provideTranslateService({
      fallbackLang: 'ru',
      loader: provideTranslateHttpLoader({
        prefix: './assets/i18n/',
        suffix: '.json'
      })
    }),

    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000'
    }),

    importProvidersFrom(FormsModule),
    CurrencyPipe
  ]
};