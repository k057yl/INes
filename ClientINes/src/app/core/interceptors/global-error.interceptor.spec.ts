import { TestBed } from '@angular/core/testing';
import { HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { globalErrorInterceptor } from './global-error.interceptor';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';
import { throwError } from 'rxjs';

describe('globalErrorInterceptor', () => {
  let toastrSpy: jasmine.SpyObj<ToastrService>;
  let translateSpy: jasmine.SpyObj<TranslateService>;

  beforeEach(() => {
    toastrSpy = jasmine.createSpyObj('ToastrService', ['error']);
    translateSpy = jasmine.createSpyObj('TranslateService', ['instant']);
    translateSpy.instant.and.callFake((key: string) => `translated_${key}`);

    TestBed.configureTestingModule({
      providers: [
        { provide: ToastrService, useValue: toastrSpy },
        { provide: TranslateService, useValue: translateSpy }
      ]
    });
  });

  it('должен игнорировать сетевые файлы перевода /assets/i18n/', (done) => {
    const req = new HttpRequest('GET', '/assets/i18n/ru.json');
    const next: HttpHandlerFn = () => throwError(() => new HttpErrorResponse({ status: 500 }));

    TestBed.runInInjectionContext(() => {
      globalErrorInterceptor(req, next).subscribe({
        error: () => {
          expect(toastrSpy.error).not.toHaveBeenCalled();
          done();
        }
      });
    });
  });

  it('должен покатывать toastr.error при ошибках сервера', (done) => {
    const req = new HttpRequest('GET', '/api/items');
    const errorResponse = new HttpErrorResponse({
      status: 400,
      error: { message: 'ITEM_NOT_FOUND' }
    });
    const next: HttpHandlerFn = () => throwError(() => errorResponse);

    TestBed.runInInjectionContext(() => {
      globalErrorInterceptor(req, next).subscribe({
        error: () => {
          expect(toastrSpy.error).toHaveBeenCalled();
          done();
        }
      });
    });
  });
});