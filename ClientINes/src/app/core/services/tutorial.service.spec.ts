import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TutorialService, TutorialStep } from './tutorial.service';
import { AuthService } from '../../features/auth/services/auth.service';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../environments/environment';

describe('TutorialService', () => {
  let service: TutorialService;
  let httpMock: HttpTestingController;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['updateLocalUserTutorial']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        TutorialService,
        { provide: AuthService, useValue: authServiceSpy },
        { provide: TranslateService, useValue: jasmine.createSpyObj('TranslateService', ['instant']) },
        { provide: ToastrService, useValue: jasmine.createSpyObj('ToastrService', ['success', 'error']) }
      ]
    });

    service = TestBed.inject(TutorialService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('markStepAsCompleted() должен обновлять туториал локально и слать запрос на бэкенд', () => {
    const step = TutorialStep.Dashboard;

    service.markStepAsCompleted(step).subscribe();

    expect(authServiceSpy.updateLocalUserTutorial).toHaveBeenCalledWith(step);

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/complete-tutorial`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ step });
    req.flush({});
  });

  it('resetTutorialsOnBackend() должен отправлять POST-запрос на сброс', () => {
    service.resetTutorialsOnBackend().subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/reset-tutorials`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });
});