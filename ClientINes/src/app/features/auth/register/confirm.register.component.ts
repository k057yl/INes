import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService, AuthResponse } from '../../../core/services/auth.service';

@Component({
  selector: 'app-confirm-register',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './confirm-register.component.html',
  styleUrl: './register.component.scss'
})
export class ConfirmRegisterComponent implements OnInit {
  private router = inject(Router);
  private authService = inject(AuthService);

  email = '';
  password = ''; 
  
  codeArray: string[] = ['', '', '', '', '', ''];
  
  message?: string;
  error?: string;

  ngOnInit() {
    const state = window.history.state;
    if (state && state.email) {
      this.email = state.email;
      this.password = state.password;
    } else {
      this.router.navigate(['/register']);
    }
  }

  onInput(event: any, index: number) {
    const input = event.target;
    const value = input.value;
    
    this.codeArray[index] = value.slice(-1);

    if (value && index < 5) {
      const nextInput = input.parentElement.children[index + 1] as HTMLInputElement;
      if (nextInput) nextInput.focus();
    }

    if (this.codeArray.every(v => v !== '')) {
      this.confirm();
    }
  }

  onKeyDown(event: KeyboardEvent, index: number) {
    if (event.key === 'Backspace' && !this.codeArray[index] && index > 0) {
      const prevInput = (event.target as HTMLElement).parentElement?.children[index - 1] as HTMLInputElement;
      if (prevInput) prevInput.focus();
    }
  }

  onPaste(event: ClipboardEvent) {
    const data = event.clipboardData?.getData('text');
    if (data && data.length === 6 && /^\d+$/.test(data)) {
      this.codeArray = data.split('');
      event.preventDefault();
      this.confirm();
    }
  }

  confirm() {
    this.error = undefined;
    const fullCode = this.codeArray.join('');
    
    if (fullCode.length < 6) return;

    this.authService.confirmRegistration(this.email, fullCode).subscribe({
      next: (res: AuthResponse) => {
        this.message = 'AUTH.SUCCESS.PASSWORD_CHANGED';
        setTimeout(() => this.router.navigate(['/main']), 1000);
      },
      error: (err: HttpErrorResponse) => {
        this.error = err.error?.error || 'AUTH.ERRORS.INVALID_OR_EXPIRED_CODE';
      }
    });
  }

  resendCode() {
    this.error = undefined;
    this.message = undefined;
    this.authService.register({ email: this.email, username: '...', password: this.password }).subscribe({
      next: () => this.message = 'AUTH.SUCCESS.OTP_SENT',
      error: (err: HttpErrorResponse) => this.error = err.error?.error || 'SYSTEM.EMAIL_SEND_FAILED'
    });
  }
}