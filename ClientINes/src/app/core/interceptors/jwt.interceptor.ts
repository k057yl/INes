import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError, Observable } from 'rxjs';
import { AuthService } from '../../features/auth/services/auth.service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<boolean>(false);

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  if (req.url.includes('.json') || req.url.includes('/assets/i18n/')) {
    return next(req);
  }

  const clonedReq = req.clone({
    withCredentials: true,
    setHeaders: {
      'X-Requested-With': 'XMLHttpRequest'
    }
  });

  return next(clonedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthRequest = req.url.includes('/auth/login') || 
                            req.url.includes('/auth/register') ||
                            req.url.includes('/auth/confirm-register') ||
                            req.url.includes('/auth/google-login') ||
                            req.url.includes('/auth/refresh') ||
                            req.url.includes('/auth/logout') ||
                            req.url.includes('/auth/forgot-password') ||
                            req.url.includes('/auth/reset-password') ||
                            req.url.includes('/auth/check-email');

      if (error.status === 401 && !isAuthRequest) {
        return handle401Error(clonedReq, next, authService);
      }
      return throwError(() => error);
    })
  );
};

function handle401Error(req: HttpRequest<any>, next: HttpHandlerFn, authService: AuthService): Observable<any> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(false);

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
      filter(status => status === true),
      take(1),
      switchMap(() => {
        return next(req);
      })
    );
  }
}