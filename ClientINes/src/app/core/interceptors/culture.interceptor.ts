import { HttpInterceptorFn } from '@angular/common/http';

export const cultureInterceptor: HttpInterceptorFn = (req, next) => {
  const lang = localStorage.getItem('lang') || 'ru';
  return next(req.clone({
    setHeaders: { 'Accept-Language': lang }
  }));
};