import { HttpInterceptorFn } from '@angular/common/http';

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