import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('jwt');

  if (req.url.includes('/auth/')) {
    return next(req);
  }

  if (token) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(authReq);
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
      if (error.status === 400 && error.error?.errors) {
        const message = Object.values(error.error.errors).flat().join('\n');
        alert(message);
      }
      return throwError(() => error);
    })
  )
}