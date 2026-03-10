import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-confirm-register',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './confirm-register.component.html',
  styleUrl: './register.component.scss'
})
export class ConfirmRegisterComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  email = '';
  password = '';
  code = '';
  message?: string;
  error?: string;

  ngOnInit() {
    const state = window.history.state;
    
    if (state && state.email) {
      this.email = state.email;
      this.password = state.password;
    } else {
      console.warn('Данные не найдены в state, возврат на регистрацию');
      this.router.navigate(['/register']);
    }
  }

  confirm() {
    this.error = undefined;
    this.http.post<{token: string}>(`${environment.apiBaseUrl}/auth/confirm-register`, {
      email: this.email,
      code: this.code
    }).subscribe({
      next: (res) => {
        this.authService.setSession(res.token); 
        this.message = 'AUTH.CONFIRM.SUCCESS_MSG';

        setTimeout(() => this.router.navigate(['/main']), 1000);
      },
      error: err => this.error = err.error?.error || 'AUTH.ERRORS.CONFIRM_FAILED'
    });
  }

  resendCode() {
    this.http.post(`${environment.apiBaseUrl}/auth/register`, {
      email: this.email,
      username: '...',
      password: this.password
    }).subscribe({
      next: () => this.message = 'Код отправлен повторно',
      error: () => this.error = 'Ошибка при переотправке'
    });
  }
}