import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router'; // Добавил активные ссылки
import { LocalizationService } from '../services/localization.service';
import { ThemeService } from '../services/theme.service'; // Импортируем наш сервис
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslatePipe],
  template: `
    <header class="header">
      <div class="brand-section">
        <h2 class="logo">INest</h2>
        <span *ngIf="userEmail" class="welcome-msg">
          {{ 'COMMON.WELCOME' | translate }}, {{ userEmail }}
        </span>
      </div>

      <nav class="nav-links">
        <a routerLink="/main" routerLinkActive="active">
          <i class="fa fa-th-large"></i> {{ 'HEADER.HOME' | translate }}
        </a>
        <a routerLink="/sales" routerLinkActive="active">
          <i class="fa fa-chart-line"></i> {{ 'HEADER.SALES' | translate }}
        </a>
        <a routerLink="/settings" routerLinkActive="active">
          <i class="fa fa-cog"></i> {{ 'HEADER.SETTINGS' | translate }}
        </a>
        <a routerLink="/login" routerLinkActive="active">
          <i class="fa fa-cog"></i> {{ 'HEADER.LOGIN' | translate }}
        </a>
        <a routerLink="/register" routerLinkActive="active">
          <i class="fa fa-cog"></i> {{ 'HEADER.REGISTER' | translate }}
        </a>
      </nav>

      <div class="actions">
        <button class="theme-toggle" (click)="toggleTheme()" [title]="'Сменить тему'">
           {{ themeService.isDarkTheme() ? '🌙' : '☀️' }}
        </button>

        <div class="lang-selector">
          <button 
            (click)="changeLang('en')" 
            [class.active]="currentLang === 'en'">EN</button>
          <button 
            (click)="changeLang('ru')" 
            [class.active]="currentLang === 'ru'">RU</button>
          <button 
            (click)="changeLang('uk')" 
            [class.active]="currentLang === 'uk'">UK</button>
        </div>
      </div>
    </header>
  `,
  styles: [`
    :host {
      --dark-blue: #0b132b;
      --navy-blue: #1c2541;
      --turquoise: #00f5d4;
      --text-light: #ffffff;
      --text-muted: #94a3b8;
      
      display: block;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem 2rem;
      background: var(--dark-blue);
      color: var(--text-light);
      box-shadow: 0 4px 12px rgba(0,0,0,0.3);
      border-bottom: 2px solid var(--navy-blue);
    }

    .logo {
      font-size: 1.5rem;
      font-weight: 800;
      color: var(--turquoise);
      letter-spacing: 1px;
      margin: 0;
    }

    .brand-section {
      display: flex;
      align-items: center;
      gap: 1.5rem;
    }

    .welcome-msg {
      color: var(--text-muted);
      font-size: 0.85rem;
      border-left: 1px solid var(--navy-blue);
      padding-left: 1rem;
    }

    .nav-links a {
      margin-left: 1.5rem;
      color: var(--text-light);
      text-decoration: none;
      font-weight: 500;
      transition: color 0.3s ease;
    }

    .nav-links a:hover, .nav-links a.active {
      color: var(--turquoise);
    }

    .actions {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .theme-toggle {
      background: var(--navy-blue);
      border: 1px solid var(--turquoise);
      border-radius: 50%;
      width: 35px;
      height: 35px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: transform 0.2s;
    }

    .theme-toggle:hover {
      transform: scale(1.1);
      box-shadow: 0 0 10px var(--turquoise);
    }

    .lang-selector button {
      background: transparent;
      border: 1px solid var(--navy-blue);
      color: var(--text-muted);
      padding: 4px 8px;
      cursor: pointer;
      font-size: 0.75rem;
      transition: all 0.2s;
    }

    .lang-selector button:first-child { border-radius: 4px 0 0 4px; }
    .lang-selector button:last-child { border-radius: 0 4px 4px 0; }

    .lang-selector button.active {
      background: var(--turquoise);
      color: var(--dark-blue);
      border-color: var(--turquoise);
      font-weight: bold;
    }
  `]
})
export class HeaderComponent {
  private loc = inject(LocalizationService);
  public themeService = inject(ThemeService);

  get currentLang() {
    return this.loc.currentLang;
  }

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

  toggleTheme() {
    this.themeService.toggleTheme();
  }
}