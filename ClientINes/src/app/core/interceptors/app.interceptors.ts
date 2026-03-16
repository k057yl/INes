import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('jwt');
  const publicApiPaths = ['/auth/login', '/auth/register', '/auth/google-login', '/auth/confirm-register', '/auth/check-email'];
  const isPublic = publicApiPaths.some(path => req.url.includes(path));

  if (!isPublic && token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next(req);
};

export const cultureInterceptor: HttpInterceptorFn = (req, next) => {
  const lang = localStorage.getItem('lang') || 'ru';
  req = req.clone({
    setHeaders: { 'Accept-Language': lang }
  });
  return next(req);
};

export const globalErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const injector = inject(Injector); // Используем Injector для ленивого получения сервиса

  // 1. ПРОВЕРКА: Если это запрос за файлом перевода (.json), просто пропускаем его
  // Это разорвет бесконечный цикл
  if (req.url.includes('.json') || req.url.includes('/assets/i18n/')) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // 2. Получаем TranslateService только здесь, когда ошибка УЖЕ случилась
      const translate = injector.get(TranslateService);

      const errorKey = error.error?.error || 'SYSTEM.DEFAULT_ERROR';
      const translatedMessage = translate.instant(errorKey);

      //alert(translatedMessage); 

      console.error(`[API Error] ${errorKey}: ${translatedMessage}`);

      return throwError(() => error);
    })
  );
};