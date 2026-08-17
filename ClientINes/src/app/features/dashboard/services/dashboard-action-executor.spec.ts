import { TestBed } from '@angular/core/testing';
import { DashboardActionExecutor } from './dashboard-action-executor.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';

describe('DashboardActionExecutor', () => {
  let executor: DashboardActionExecutor;
  let toastrSpy: jasmine.SpyObj<ToastrService>;
  let translateSpy: jasmine.SpyObj<TranslateService>;

  beforeEach(() => {
    toastrSpy = jasmine.createSpyObj('ToastrService', ['success', 'error']);
    translateSpy = jasmine.createSpyObj('TranslateService', ['instant']);
    translateSpy.instant.and.callFake((key: string) => `translated_${key}`);

    TestBed.configureTestingModule({
      providers: [
        DashboardActionExecutor,
        { provide: ToastrService, useValue: toastrSpy },
        { provide: TranslateService, useValue: translateSpy }
      ]
    });

    executor = TestBed.inject(DashboardActionExecutor);
  });

  it('должен вызывать toastr.success и onSuccess каллбэк при успешном выполнении', () => {
    const onSuccessSpy = jasmine.createSpy('onSuccess');
    const mockObs$ = of({ id: 123 });

    executor.run(mockObs$, 'SUCCESS.KEY', onSuccessSpy);

    expect(translateSpy.instant).toHaveBeenCalledWith('SUCCESS.KEY');
    expect(toastrSpy.success).toHaveBeenCalledWith('translated_SUCCESS.KEY');
    expect(onSuccessSpy).toHaveBeenCalledWith({ id: 123 });
  });

  it('должен обрабатывать ошибки API и вызывать toastr.error', () => {
    const errorResponse = { error: { message: 'CUSTOM_ERROR_KEY' } };
    const mockObs$ = throwError(() => errorResponse);

    executor.run(mockObs$, 'SUCCESS.KEY');

    expect(translateSpy.instant).toHaveBeenCalledWith('CUSTOM_ERROR_KEY');
    expect(toastrSpy.error).toHaveBeenCalledWith('translated_CUSTOM_ERROR_KEY');
  });
});