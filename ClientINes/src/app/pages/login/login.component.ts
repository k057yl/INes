import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>Вход</h2>
    <form (ngSubmit)="login()">
      <input [(ngModel)]="email" name="email" placeholder="Email" required />
      <input [(ngModel)]="password" name="password" type="password" placeholder="Пароль" required />
      <button type="submit">Войти</button>
    </form>
    <p *ngIf="error" style="color:red">{{ error }}</p>
  `
})
export class LoginComponent {
  email = '';
  password = '';
  error?: string;

  constructor(private http: HttpClient, private router: Router) {}

  login() {
    this.error = undefined;

    const payload = {
      Email: this.email,
      Password: this.password
    };

    this.http.post<{ token: string | "unconfirmed" }>(
      `${environment.apiBaseUrl}/auth/login`,
      payload
    ).subscribe({
      next: res => {
        if (res.token === "unconfirmed") {
          this.error = "Email не подтвержден. Проверьте почту.";
          return;
        }

        localStorage.setItem('token', res.token);
        this.router.navigate(['/home']);
      },
      error: err => {
        this.error = err.error?.error || err.error || 'Ошибка входа';
      }
    });
  }
}