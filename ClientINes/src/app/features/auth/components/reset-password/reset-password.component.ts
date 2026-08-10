import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, TranslateModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  resetForm: FormGroup;
  email = '';
  otpCode = '';
  showPassword = false;
  
  message?: string;
  error?: string;

  constructor() {
    this.resetForm = this.fb.group({
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

  ngOnInit() {
    const state = window.history.state;
    if (state && state.email) {
      this.email = state.email;
    } else {
      this.router.navigate(['/forgot-password']);
    }
  }

  onOtpChange(value: string) {
    this.otpCode = value.replace(/\D/g, '').slice(0, 6);
  }

  togglePassword() { this.showPassword = !this.showPassword; }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value ? null : { 'mismatch': true };
  }

  submit() {
    if (this.resetForm.invalid || this.otpCode.length < 6) return;

    this.error = undefined;
    const newPassword = this.resetForm.get('password')?.value;

    this.authService.resetPassword({
      email: this.email,
      token: this.otpCode,
      newPassword: newPassword
    }).subscribe({
      next: () => {
        this.message = 'AUTH.SUCCESS.PASSWORD_CHANGED';
        setTimeout(() => this.router.navigate(['/login']), 1200);
      },
      error: (err: HttpErrorResponse) => {
        this.error = err.error?.error || 'SYSTEM.DEFAULT_ERROR';
      }
    });
  }

  resendCode() {
    this.error = undefined;
    this.message = undefined;
    this.authService.forgotPassword({ email: this.email }).subscribe({
      next: () => this.message = 'AUTH.SUCCESS.OTP_SENT',
      error: (err: HttpErrorResponse) => this.error = err.error?.error || 'SYSTEM.EMAIL_SEND_FAILED'
    });
  }
}