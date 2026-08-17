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

  const token = localStorage.getItem('token');
  const headers: Record<string, string> = {
    'X-Requested-With': 'XMLHttpRequest'
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const clonedReq = req.clone({
    withCredentials: true,
    setHeaders: headers
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
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      switchMap(() => {
        isRefreshing = false;
        refreshTokenSubject.next(true);

        const newToken = localStorage.getItem('token');
        const retryReq = newToken 
          ? req.clone({ setHeaders: { ...req.headers, Authorization: `Bearer ${newToken}` } })
          : req;

        return next(retryReq);
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
          const newToken = localStorage.getItem('token');
          const retryReq = newToken 
            ? req.clone({ setHeaders: { ...req.headers, Authorization: `Bearer ${newToken}` } })
            : req;

          return next(retryReq);
        }
        return throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' }));
      })
    );
  }
}