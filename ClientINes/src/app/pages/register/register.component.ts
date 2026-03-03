import { Component, inject, OnInit, AfterViewInit, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';
import { environment } from '../../../environments/environment';
import { emailUniqueValidator } from '../../validators/email-unique.validator';
import { AuthService } from '../../services/auth.service';

declare var google: any;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit, AfterViewInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private ngZone = inject(NgZone);

  registerForm!: FormGroup;
  message?: string;
  error?: string;

  showGoogle = false;

  toggleGoogle() {
    this.showGoogle = !this.showGoogle;
    if (this.showGoogle) {
      setTimeout(() => this.initGoogleSignIn(), 50);
    }
  }

  ngOnInit() {
    this.initForm();
  }

  ngAfterViewInit() {
    this.initGoogleSignIn();
  }

  private initGoogleSignIn() {
    const clientId = '477633498097-9ovq0psecdt4iu5lhh631dfjofgdlt2e.apps.googleusercontent.com';
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
        next: () => this.router.navigate(['/dashboard']),
        error: (err) => {
          this.error = 'AUTH.ERRORS.GOOGLE_FAILED';
          console.error('Google login error', err);
        }
      });
    });
  }

  private initForm() {
    this.registerForm = this.fb.group({
      username: ['', [Validators.required]],
      email: ['', 
        [Validators.required, Validators.email], 
        [emailUniqueValidator(this.authService)]
      ],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { 'mismatch': true };
  }

  register() {
    if (this.registerForm.invalid) return;

    this.error = undefined;
    this.message = undefined;

    const { username, email, password } = this.registerForm.getRawValue();

    this.http.post(`${environment.apiBaseUrl}/auth/register`, {
      username,
      email,
      password
    }).subscribe({
      next: () => {
        this.message = 'AUTH.REGISTER.SUCCESS_MSG';
        setTimeout(() => {
          this.router.navigate(['/confirm-register'], { state: { email, password } });
        }, 1500);
      },
      error: err => this.error = err.error?.error || 'AUTH.ERRORS.REGISTER_FAILED'
    });
  }
}