import { Component, inject, OnInit } from '@angular/core';

import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-confirm-register',
  standalone: true,
  imports: [FormsModule, TranslateModule],
  templateUrl: './confirm-register.component.html',
  styleUrl: './register.component.scss'
})
export class ConfirmRegisterComponent implements OnInit {
  private router = inject(Router);
  private authService = inject(AuthService);

  email = '';
  otpCode = ''; 
  
  message?: string;
  error?: string;

  ngOnInit() {
    const state = window.history.state;
    if (state && state.email) {
      this.email = state.email;
    } else {
      this.router.navigate(['/register']);
    }
  }

  onOtpChange(value: string) {
    this.otpCode = value.replace(/\D/g, '').slice(0, 6);
    
    if (this.otpCode.length === 6) {
      this.confirm();
    }
  }

  confirm() {
    if (this.otpCode.length < 6) return;
    this.error = undefined;

    this.authService.confirmRegistration(this.email, this.otpCode).subscribe({
      next: () => {
        this.message = 'CONFIRM_REGISTRATION_PAGE.SUCCESS';
        setTimeout(() => this.router.navigate(['/dashboard']), 1000);
      },
      error: (err: HttpErrorResponse) => {
        this.error = err.error?.error || 'CONFIRM_REGISTRATION_PAGE.ERROR';
      }
    });
  }

  resendCode() {
    this.error = undefined;
    this.message = undefined;
    
    this.authService.resendCode({ email: this.email }).subscribe({
      next: () => this.message = 'AUTH.SUCCESS.OTP_SENT',
      error: (err: HttpErrorResponse) => this.error = err.error?.error || 'SYSTEM.EMAIL_SEND_FAILED'
    });
  }
}