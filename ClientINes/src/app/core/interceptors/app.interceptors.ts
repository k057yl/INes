import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError, Observable } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<boolean | null>(null);

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  if (req.url.includes('.json') || req.url.includes('/assets/i18n/')) {
    return next(req);
  }

  const clonedReq = req.clone({
    withCredentials: true,
    setHeaders: { 'X-Requested-With': 'XMLHttpRequest' }
  });

  return next(clonedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/auth/login') && !req.url.includes('/auth/refresh')) {
        return handle401Error(clonedReq, next, authService);
      }
      return throwError(() => error);
    })
  );
};

function handle401Error(req: HttpRequest<any>, next: HttpHandlerFn, authService: AuthService): Observable<any> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap(() => {
        isRefreshing = false;
        refreshTokenSubject.next(true);
        return next(req);
      }),
      catchError((err) => {
        isRefreshing = false;
        authService.logout().subscribe();
        return throwError(() => err);
      })
    );
  } else {
    return refreshTokenSubject.pipe(
      filter(done => done !== null),
      take(1),
      switchMap(() => next(req))
    );
  }
}

export const cultureInterceptor: HttpInterceptorFn = (req, next) => {
  const lang = localStorage.getItem('lang') || 'ru';
  return next(req.clone({
    setHeaders: { 'Accept-Language': lang }
  }));
};

export const globalErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const injector = inject(Injector);
  const toastr = inject(ToastrService);

  if (req.url.includes('.json') || req.url.includes('/assets/i18n/')) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && (req.url.includes('/auth/me') || req.url.includes('/auth/login'))) {
        return throwError(() => error);
      }

      const translate = injector.get(TranslateService);
      const errorKey = error.error?.error || 'SYSTEM.DEFAULT_ERROR';
      const translatedMessage = translate.instant(errorKey);
      const translatedTitle = translate.instant('SYSTEM.DEFAULT_ERROR');

      toastr.error(translatedMessage, translatedTitle, {
        enableHtml: true,
        closeButton: true,
        timeOut: 5000
      });

      console.error(`[API Error] ${errorKey}: ${translatedMessage}`);
      
      return throwError(() => error);
    })
  );
};