import { Component, inject, OnInit, AfterViewInit, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { environment } from '../../../../environments/environment';
import { emailUniqueValidator } from '../../../shared/validators/email-unique.validator';
import { AuthService } from '../../../core/services/auth.service';

declare var google: any;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent implements OnInit, AfterViewInit {
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private ngZone = inject(NgZone);

  registerForm!: FormGroup;
  message?: string;
  error?: string;

  showGoogle = false;
  showPassword = false;

  ngOnInit() {
    this.initForm();
  }

  ngAfterViewInit() {
    this.initGoogleSignIn();
  }

  private initForm() {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    this.registerForm = this.fb.group({
      username: ['', [
        Validators.required, 
        Validators.minLength(3),
        Validators.pattern(/^[a-zA-Z0-9]*$/)
      ]],
      email: ['', 
        [Validators.required, Validators.pattern(emailRegex)], 
        [emailUniqueValidator()]
      ],
      password: ['', [
        Validators.required, 
        Validators.minLength(6),
        Validators.pattern(/^[\u0000-\u007F]+$/),
        Validators.pattern(/[A-Z]/),
        Validators.pattern(/[0-9]/),
        Validators.pattern(/[^a-zA-Z0-9]/)
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  hasForbiddenChars(): boolean {
    const password = this.registerForm.get('password')?.value || '';
    if (!password) return false;
    return /[^\u0000-\u007F]/.test(password);
  }

  checkPasswordRequirement(regex: string): boolean {
    const password = this.registerForm.get('password')?.value || '';
    if (!password) return false;
    return new RegExp(regex).test(password);
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value ? null : { 'mismatch': true };
  }

  register() {
    if (this.registerForm.invalid) return;

    this.error = undefined;
    this.message = undefined;

    const dto = this.registerForm.getRawValue();

    this.authService.register(dto).subscribe({
      next: () => {
        this.message = 'AUTH.SUCCESS.OTP_SENT'; 
        setTimeout(() => {
          this.router.navigate(['/confirm-register'], { 
            state: { email: dto.email, password: dto.password } 
          });
        }, 1500);
      },
      error: (err) => {
        this.error = err.error?.error || 'SYSTEM.DEFAULT_ERROR';
      }
    });
  }

  // --- UI Helpers ---
  togglePassword() { this.showPassword = !this.showPassword; }
  
  toggleGoogle() {
    this.showGoogle = !this.showGoogle;
    if (this.showGoogle) setTimeout(() => this.initGoogleSignIn(), 50);
  }

  private initGoogleSignIn() {
    const clientId = environment.googleClientId;
    const renderAction = () => {
      if (typeof google !== 'undefined' && google.accounts) {
        google.accounts.id.initialize({
          client_id: clientId,
          callback: (response: any) => this.handleGoogleLogin(response),
          auto_select: false
        });
        const btnContainer = document.getElementById('googleBtn');
        if (btnContainer) {
          btnContainer.innerHTML = ''; 
          google.accounts.id.renderButton(btnContainer, { 
            theme: 'outline', size: 'large', width: 370 
          });
        }
      } else {
        setTimeout(renderAction, 100);
      }
    };
    renderAction();
  }

  private handleGoogleLogin(response: any) {
    this.ngZone.run(() => {
      this.authService.googleLogin(response.credential).subscribe({
        next: () => this.router.navigate(['/main']),
        error: (err) => this.error = err.error?.error || 'AUTH.ERRORS.GOOGLE_AUTH_FAILED'
      });
    });
  }
}