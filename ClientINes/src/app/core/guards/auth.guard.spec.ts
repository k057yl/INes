import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { authGuard, guestGuard, adminGuard } from './auth.guard';
import { AuthService } from '../../features/auth/services/auth.service';
import { BehaviorSubject } from 'rxjs';

describe('Auth Guards', () => {
  let userSubject: BehaviorSubject<any>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    userSubject = new BehaviorSubject<any>(undefined);
    routerSpy = jasmine.createSpyObj('Router', ['parseUrl']);
    routerSpy.parseUrl.and.callFake((url: string) => ({ url } as unknown as UrlTree));

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: routerSpy },
        { provide: AuthService, useValue: { user$: userSubject.asObservable() } }
      ]
    });
  });

  describe('authGuard', () => {
    it('должен пропускать (true), если юзер залогинен', (done) => {
      userSubject.next({ id: 'u-1' });

      TestBed.runInInjectionContext(() => {
        (authGuard({} as any, {} as any) as any).subscribe((res: boolean | UrlTree) => {
          expect(res).toBeTrue();
          done();
        });
      });
    });

    it('должен редиректить на /login, если юзер не залогинен', (done) => {
      userSubject.next(null);

      TestBed.runInInjectionContext(() => {
        (authGuard({} as any, {} as any) as any).subscribe(() => {
          expect(routerSpy.parseUrl).toHaveBeenCalledWith('/login');
          done();
        });
      });
    });
  });

  describe('adminGuard', () => {
    it('должен пропускать только юзеров с ролью inest_admin', (done) => {
      userSubject.next({ roles: ['inest_admin'] });

      TestBed.runInInjectionContext(() => {
        (adminGuard({} as any, {} as any) as any).subscribe((res: boolean) => {
          expect(res).toBeTrue();
          done();
        });
      });
    });

    it('должен редиректить обычных юзеров на /dashboard', (done) => {
      userSubject.next({ roles: ['user'] });

      TestBed.runInInjectionContext(() => {
        (adminGuard({} as any, {} as any) as any).subscribe(() => {
          expect(routerSpy.parseUrl).toHaveBeenCalledWith('/dashboard');
          done();
        });
      });
    });
  });
});