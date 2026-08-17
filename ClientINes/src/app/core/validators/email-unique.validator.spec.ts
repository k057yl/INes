import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { emailUniqueValidator } from './email-unique.validator';
import { AuthService } from '../../features/auth/services/auth.service';
import { Observable, of } from 'rxjs';

describe('emailUniqueValidator', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['checkEmailUnique']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceSpy }
      ]
    });
  });

  it('должен возвращать null, если email уникален', fakeAsync(() => {
    authServiceSpy.checkEmailUnique.and.returnValue(of(true));
    const control = new FormControl('test@inest.com');

    let result: any = undefined;
    TestBed.runInInjectionContext(() => {
      const validator$ = emailUniqueValidator()(control) as Observable<any>;
      validator$.subscribe((res: any) => result = res);
    });

    tick(400);
    expect(result).toBeNull();
  }));

  it('должен возвращать { emailExists: true }, если email занят', fakeAsync(() => {
    authServiceSpy.checkEmailUnique.and.returnValue(of(false));
    const control = new FormControl('busy@inest.com');

    let result: any = undefined;
    TestBed.runInInjectionContext(() => {
      const validator$ = emailUniqueValidator()(control) as Observable<any>;
      validator$.subscribe((res: any) => result = res);
    });

    tick(400);
    expect(result).toEqual({ emailExists: true });
  }));
});