import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError, Observable } from 'rxjs';
import { AuthService } from '../../features/auth/services/auth.service';

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
      if (error.status === 401 && !req.url.includes('/auth/login') && !req.url.includes('/auth/refresh') && !req.url.includes('/auth/logout')) {
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
        refreshTokenSubject.next(false);
        
        authService.clearLocalSession(); 

        return throwError(() => err);
      })
    );
  } else {
    return refreshTokenSubject.pipe(
      filter(done => done !== null),
      take(1),
      switchMap((success) => {
        if (success) {
          return next(req);
        }
        return throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' }));
      })
    );
  }
}