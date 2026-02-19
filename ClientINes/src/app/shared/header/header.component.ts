import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LocalizationService } from '../../services/localization.service';
import { ThemeService } from '../../services/theme.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  private loc = inject(LocalizationService);
  public themeService = inject(ThemeService);

  // Состояние мобильного меню
  isMenuOpen = signal(false);

  get currentLang() { return this.loc.currentLang; }

  get userEmail(): string | null {
    const token = localStorage.getItem('token');
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] 
             || payload.name || payload.sub || 'User';
    } catch { return null; }
  }

  changeLang(lang: string) { this.loc.setLanguage(lang); }
  toggleTheme() { this.themeService.toggleTheme(); }
  
  toggleMenu() { this.isMenuOpen.set(!this.isMenuOpen()); }
  closeMenu() { this.isMenuOpen.set(false); }
}