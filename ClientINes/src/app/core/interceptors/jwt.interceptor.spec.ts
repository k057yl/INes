import { TestBed } from '@angular/core/testing';
import { HttpRequest, HttpHandlerFn, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { jwtInterceptor } from './jwt.interceptor';
import { AuthService } from '../../features/auth/services/auth.service';
import { of, throwError } from 'rxjs';

describe('jwtInterceptor', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['refreshToken', 'clearLocalSession']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceSpy }
      ]
    });
  });

  it('должен добавлять withCredentials и X-Requested-With к запросам', (done) => {
    const req = new HttpRequest('GET', '/api/items');
    const next: HttpHandlerFn = (clonedReq) => {
      expect(clonedReq.withCredentials).toBeTrue();
      expect(clonedReq.headers.get('X-Requested-With')).toBe('XMLHttpRequest');
      return of(new HttpResponse({ status: 200 }));
    };

    TestBed.runInInjectionContext(() => {
      jwtInterceptor(req, next).subscribe(() => done());
    });
  });

  it('при 401 ошибке должен пытаться обновить токен через refreshToken()', (done) => {
    const req = new HttpRequest('GET', '/api/items');
    const errorResponse = new HttpErrorResponse({ status: 401 });
    let callsCount = 0;

    const next: HttpHandlerFn = () => {
      callsCount++;
      if (callsCount === 1) return throwError(() => errorResponse);
      return of(new HttpResponse({ status: 200 }));
    };

    authServiceSpy.refreshToken.and.returnValue(of({}));

    TestBed.runInInjectionContext(() => {
      jwtInterceptor(req, next).subscribe(() => {
        expect(authServiceSpy.refreshToken).toHaveBeenCalled();
        done();
      });
    });
  });
});