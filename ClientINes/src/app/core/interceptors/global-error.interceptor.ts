import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

export const globalErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const injector = inject(Injector);
  const toastr = inject(ToastrService);

  if (req.url.includes('.json') || req.url.includes('/assets/i18n/')) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 || error.status === 403) {
        return throwError(() => error);
      }

      let errorKey = 'SYSTEM.DEFAULT_ERROR';

      if (error.error?.details) {
        const details = error.error.details;
        const firstField = Object.keys(details)[0];
        if (firstField && details[firstField]?.length > 0) {
          errorKey = details[firstField][0];
        }
      } else if (error.error?.message) {
        errorKey = error.error.message;
      } else if (error.error?.error) {
        errorKey = error.error.error;
      }

      const translate = injector.get(TranslateService);
      const translatedMessage = translate.instant(errorKey);
      const translatedTitle = translate.instant('SYSTEM.DEFAULT_ERROR');

      toastr.error(translatedMessage, translatedTitle, {
        enableHtml: true,
        closeButton: true,
        timeOut: 5000
      });

      console.error(`[API Error] ${errorKey}: ${translatedMessage}`);
      
      if (error.error && typeof error.error === 'object') {
        error.error.error = errorKey;
      }

      return throwError(() => error);
    })
  );
};