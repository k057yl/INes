import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { TutorialStep } from '../../../core/services/tutorial.service';
import { environment } from '../../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiBaseUrl}/auth`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('login() должен отправлять POST и сразу делать checkAuth()', (done) => {
    const mockUser = { id: 'u1', email: 'test@inest.com' } as any;

    service.login('test@inest.com', 'Pass123!').subscribe(user => {
      expect(user).toEqual(mockUser);
      expect(service.isLoggedIn()).toBeTrue();
      done();
    });

    // 1-й запрос: POST /login
    const loginReq = httpMock.expectOne(`${apiUrl}/login`);
    expect(loginReq.request.method).toBe('POST');
    loginReq.flush({});

    // 2-й запрос: GET /me (вызывается цепочкой через switchMap)
    const meReq = httpMock.expectOne(`${apiUrl}/me`);
    expect(meReq.request.method).toBe('GET');
    meReq.flush(mockUser);
  });

  it('updateLocalUserTutorial должен корректно применять битовую маску', () => {
    // Сетим начального юзера с пройденным 0 шагом
    (service as any).userSubject.next({ completedTutorials: 0 });

    service.updateLocalUserTutorial(TutorialStep.ItemForm); // Допустим Step = 1

    expect((service as any).userSubject.value.completedTutorials).toBe(TutorialStep.ItemForm);
  });

  it('clearLocalSession должен сбрасывать состояние юзера в null', () => {
    (service as any).userSubject.next({ id: '123' });
    service.clearLocalSession();
    expect(service.isLoggedIn()).toBeFalse();
  });
});