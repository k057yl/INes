import { Component, inject, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocalizationService } from '../../services/localization.service';
import { ThemeService } from '../../../core/services/theme.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  public authService = inject(AuthService);
  public loc = inject(LocalizationService);
  public themeService = inject(ThemeService);
  private router = inject(Router);

  isMenuOpen = signal(false);
  isLangMenuOpen = signal(false);
  user$ = this.authService.user$;

  get currentLang() { return this.loc.currentLang; }

  onLogout() {
    this.authService.logout().subscribe({
      next: () => this.handleLogoutRedirect(),
      error: () => this.handleLogoutRedirect() 
    });
  }

  private handleLogoutRedirect() {
    this.isMenuOpen.set(false);
    this.router.navigate(['/login']);
  }

  toggleLangMenu(event: MouseEvent) {
    event.stopPropagation();
    this.isLangMenuOpen.set(!this.isLangMenuOpen());
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    this.isLangMenuOpen.set(false);
  }

  changeLang(lang: string) {
    this.loc.setLanguage(lang);
    this.isLangMenuOpen.set(false);
  }

  toggleTheme() { this.themeService.toggleTheme(); }
  toggleMenu() { this.isMenuOpen.set(!this.isMenuOpen()); }
  closeMenu() { this.isMenuOpen.set(false); }
}