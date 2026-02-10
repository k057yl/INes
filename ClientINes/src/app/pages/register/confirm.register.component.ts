import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-confirm-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>Подтверждение регистрации</h2>
    <form (ngSubmit)="confirm()">
      <input [(ngModel)]="code" name="code" placeholder="Код из письма" required />
      <button type="submit">Подтвердить</button>
    </form>
    <p *ngIf="message" style="color:green">{{ message }}</p>
    <p *ngIf="error" style="color:red">{{ error }}</p>
  `
})
export class ConfirmRegisterComponent {
  email = '';
  password = '';
  code = '';
  message?: string;
  error?: string;

  constructor(private http: HttpClient, private router: Router, private route: ActivatedRoute) {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      this.password = params['password'] || '';
    });
  }

  confirm() {
    this.error = undefined;
    this.message = undefined;

    this.http.post(`${environment.apiBaseUrl}/auth/confirm-register`, {
      email: this.email,
      password: this.password,
      code: this.code
    }).subscribe({
      next: () => {
        this.message = 'Заебок, вы зарегистрированы!';
        setTimeout(() => this.router.navigate(['/home']), 1500);
      },
      error: err => this.error = err.error?.error || 'Ошибка подтверждения'
    });
  }
}