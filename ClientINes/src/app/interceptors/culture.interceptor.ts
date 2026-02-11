import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class CultureInterceptor implements HttpInterceptor {

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const lang = localStorage.getItem('lang') || 'en';

    const cloned = req.clone({
      setHeaders: {
        'Accept-Language': lang
      }
    });

    return next.handle(cloned);
  }
}