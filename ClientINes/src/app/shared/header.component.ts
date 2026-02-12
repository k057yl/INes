import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LocalizationService } from '../services/localization.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
  template: `
    <header class="header">
      <div class="brand-section">
        <h2>INest</h2>
        <span *ngIf="userEmail" class="welcome-msg">
          Привет, {{ userEmail }}
        </span>
      </div>

      <nav>
        <a routerLink="/home">{{ 'HEADER.HOME' | translate }}</a>
        <a routerLink="/login">{{ 'HEADER.LOGIN' | translate }}</a>
        <a routerLink="/register">{{ 'HEADER.REGISTER' | translate }}</a>
        <a routerLink="/create-item">{{ 'HEADER.TEST' | translate }}</a>
        <a routerLink="/create-category">{{ 'HEADER.TEST' | translate }}</a>
      </nav>

      <div class="lang">
        <button (click)="changeLang('en')">EN</button>
        <button (click)="changeLang('ru')">RU</button>
        <button (click)="changeLang('uk')">UK</button>
      </div>
    </header>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 15px;
      background: #222;
      color: white;
    }
    .brand-section {
      display: flex;
      align-items: center;
      gap: 20px;
    }
    .welcome-msg {
      color: #ff4d4d;
      font-weight: bold;
      font-size: 0.9rem;
    }
    nav a {
      margin-left: 15px;
      color: white;
      text-decoration: none;
    }
    .lang button {
      margin-left: 5px;
      cursor: pointer;
    }
  `]
})
export class HeaderComponent {
  private loc = inject(LocalizationService);

  get userEmail(): string | null {
    const token = localStorage.getItem('token');
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      
      return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] 
            || payload.name 
            || payload.sub 
            || 'Пользователь';
    } catch {
      return null;
    }
  }

  changeLang(lang: string) {
    this.loc.setLanguage(lang);
  }
}