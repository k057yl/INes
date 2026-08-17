import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../services/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['register', 'checkEmailUnique']);
    authServiceSpy.checkEmailUnique.and.returnValue(of(true));

    await TestBed.configureTestingModule({
      imports: [RegisterComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('форма должна быть невалидной по умолчанию', () => {
    expect(component.registerForm.valid).toBeFalse();
  });

  it('валидатор пароля должен требовать заглавную букву, цифру и спецсимвол', () => {
    const passwordControl = component.registerForm.get('password');

    passwordControl?.setValue('simple'); // Без цифр и спецсимволов
    expect(passwordControl?.valid).toBeFalse();

    passwordControl?.setValue('Password123!'); // Валидный
    expect(passwordControl?.valid).toBeTrue();
  });

  it('passwordMatchValidator должен выставлять ошибку mismatch, если пароли не совпадают', () => {
    component.registerForm.get('password')?.setValue('Password123!');
    component.registerForm.get('confirmPassword')?.setValue('DifferentPass123!');

    expect(component.registerForm.errors?.['mismatch']).toBeTrue();
  });

  it('hasForbiddenChars должен находить кириллицу в пароле', () => {
    component.registerForm.get('password')?.setValue('Пароль123!');
    expect(component.hasForbiddenChars()).toBeTrue();
  });
});