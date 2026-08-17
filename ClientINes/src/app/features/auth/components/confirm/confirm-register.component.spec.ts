import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmRegisterComponent } from './confirm.register.component';
import { AuthService } from '../../services/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';

describe('ConfirmRegisterComponent', () => {
  let component: ConfirmRegisterComponent;
  let fixture: ComponentFixture<ConfirmRegisterComponent>;
  let router: Router;
  let authSpy: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    authSpy = jasmine.createSpyObj('AuthService', ['confirmRegistration']);
    authSpy.confirmRegistration.and.returnValue(of({}));

    await TestBed.configureTestingModule({
      imports: [ConfirmRegisterComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmRegisterComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
  });

  it('onOtpChange должен оставлять только первые 6 цифр', () => {
    component.onOtpChange('12abc345678');
    expect(component.otpCode).toBe('123456');
  });

  it('ngOnInit должен редиректить на /register, если в history.state нет email', () => {
    spyOn(router, 'navigate');
    component.ngOnInit();
    expect(router.navigate).toHaveBeenCalledWith(['/register']);
  });
});