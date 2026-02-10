import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h1>Привет, {{ email || 'гость' }}!</h1>
    <p>Email: {{ email || '-' }}</p>
    <p *ngIf="roles?.length">Роли: {{ roles?.join(', ') }}</p>
    <button (click)="logout()">Выйти</button>
  `
})
export class HomeComponent implements OnInit {
  email?: string;
  roles?: string[];

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.get<{ email: string; roles: string[] }>('/auth/me').subscribe({
      next: res => {
        this.email = res.email;
        this.roles = res.roles;
      },
      error: err => console.error('Ошибка получения данных пользователя', err)
    });
  }

  logout() {
    const token = localStorage.getItem('token');
    if (!token) return;

    localStorage.removeItem('token');
    location.href = '/login';
  }
}