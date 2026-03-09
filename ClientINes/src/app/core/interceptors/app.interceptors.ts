import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('jwt');

  const publicApiPaths = [
    '/auth/login',
    '/auth/register',
    '/auth/google-login',
    '/auth/confirm-register',
    '/auth/check-email'
  ];

  const isPublic = publicApiPaths.some(path => req.url.includes(path));

  if (!isPublic && token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req);
};

export const cultureInterceptor: HttpInterceptorFn = (req, next) => {
  const lang = localStorage.getItem('lang') || 'ru';
  
  req = req.clone({
    setHeaders: {
      'Accept-Language': lang
    }
  });
  return next(req);
};

export const globalErrorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      return throwError(() => error);
    })
  )
};