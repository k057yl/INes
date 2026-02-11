import { Component } from '@angular/core';
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
      <h2>INest</h2>

      <nav>
        <a routerLink="/home">{{ 'HEADER.HOME' | translate }}</a>
        <a routerLink="/login">{{ 'HEADER.LOGIN' | translate }}</a>
        <a routerLink="/register">{{ 'HEADER.REGISTER' | translate }}</a>
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

  constructor(private loc: LocalizationService) {}

  changeLang(lang: string) {
    this.loc.setLanguage(lang);
  }
}