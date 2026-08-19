import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError, Observable } from 'rxjs';
import { AuthService } from '../../features/auth/services/auth.service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

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
      switchMap((res: any) => {
        isRefreshing = false;
        
        const newToken = res?.data?.token || res?.token;
        if (newToken) {
          localStorage.setItem('token', newToken);
          refreshTokenSubject.next(newToken);
        } else {
          authService.clearLocalSession();
          return throwError(() => new Error('No token in refresh response'));
        }

        const retryReq = req.clone({
          withCredentials: true,
          setHeaders: { Authorization: `Bearer ${newToken}` }
        });

        return next(retryReq);
      }),
      catchError((err) => {
        isRefreshing = false;
        refreshTokenSubject.next(null);

        authService.clearLocalSession();
        return throwError(() => err);
      })
    );
  } else {
    return refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap((token) => {
        const retryReq = req.clone({
          withCredentials: true,
          setHeaders: { Authorization: `Bearer ${token}` }
        });
        return next(retryReq);
      })
    );
  }
}