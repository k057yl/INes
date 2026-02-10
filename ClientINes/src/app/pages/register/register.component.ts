import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>Регистрация</h2>
    <form (ngSubmit)="register()">
      <input [(ngModel)]="username" name="username" placeholder="Имя пользователя" required />
      <input [(ngModel)]="email" name="email" placeholder="Email" required />
      <input [(ngModel)]="password" name="password" type="password" placeholder="Пароль" required />
      <input [(ngModel)]="confirmPassword" name="confirmPassword" type="password" placeholder="Повторите пароль" required />
      <button type="submit">Зарегистрироваться</button>
    </form>
    <p *ngIf="message" style="color:green">{{ message }}</p>
    <p *ngIf="error" style="color:red">{{ error }}</p>
  `
})
export class RegisterComponent {
  username = '';
  email = '';
  password = '';
  confirmPassword = '';
  message?: string;
  error?: string;

  constructor(private http: HttpClient, private router: Router) {}

  register() {
    this.error = undefined;
    this.message = undefined;

    if (this.password !== this.confirmPassword) {
      this.error = 'Пароли не совпадают';
      return;
    }

    this.http.post(`${environment.apiBaseUrl}/auth/register`, {
      username: this.username,
      email: this.email,
      password: this.password
    }).subscribe({
      next: () => {
        this.message = 'Одноразовый код отправлен на почту';
        setTimeout(() => this.router.navigate(['/confirm-register'], { queryParams: { email: this.email } }), 1000);
      },
      error: err => this.error = err.error?.error || 'Ошибка регистрации'
    });
  }
}