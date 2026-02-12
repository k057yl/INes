import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

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

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  login() {

    this.error = undefined;

    this.authService.login(this.email, this.password)
      .subscribe({
        next: () => {
          this.router.navigate(['/home']);
        },
        error: err => {

          if (err.error === 'unconfirmed') {
            this.error = 'Email не подтвержден. Проверьте почту.';
            return;
          }

          this.error =
            err.error?.error ||
            err.error ||
            'Ошибка входа';
        }
      });
  }
}