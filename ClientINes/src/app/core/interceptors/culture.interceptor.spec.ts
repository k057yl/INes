import { TestBed } from '@angular/core/testing';
import { HttpRequest, HttpHandlerFn, HttpResponse } from '@angular/common/http';
import { cultureInterceptor } from './culture.interceptor';
import { of } from 'rxjs';

describe('cultureInterceptor', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('должен подставлять язык из localStorage в заголовок Accept-Language', (done) => {
    localStorage.setItem('lang', 'uk');
    const req = new HttpRequest('GET', '/api/items');

    const next: HttpHandlerFn = (clonedReq) => {
      expect(clonedReq.headers.get('Accept-Language')).toBe('uk');
      return of(new HttpResponse({ status: 200 }));
    };

    cultureInterceptor(req, next).subscribe(() => done());
  });

  it('должен использовать ru по умолчанию, если localStorage пуст', (done) => {
    const req = new HttpRequest('GET', '/api/items');

    const next: HttpHandlerFn = (clonedReq) => {
      expect(clonedReq.headers.get('Accept-Language')).toBe('ru');
      return of(new HttpResponse({ status: 200 }));
    };

    cultureInterceptor(req, next).subscribe(() => done());
  });
});