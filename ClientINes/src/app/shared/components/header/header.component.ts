import { Component, inject, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocalizationService } from '../../services/localization.service';
import { ThemeService } from '../../../core/services/theme.service';
import { TranslateModule } from '@ngx-translate/core';

import { MainPageModalService } from '../../../features/inventory/main/main-page.modal.service';//----------------

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

  public modalService = inject(MainPageModalService);//-----------------

  isMenuOpen = signal(false);
  isLangMenuOpen = signal(false);

  isCreateMenuOpen = signal(false); //---------------

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

  toggleCreateMenu(event: MouseEvent) {
    event.stopPropagation();
    this.isLangMenuOpen.set(false);
    this.isCreateMenuOpen.set(!this.isCreateMenuOpen());
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    this.isLangMenuOpen.set(false);
    this.isCreateMenuOpen.set(false);
  }

  // Методы вызова глобальных модалок
  openCreateItem() {
    this.modalService.openCreateItem(); // Вызываем без параметров = корень
    this.isCreateMenuOpen.set(false);
  }

  openCreateLocation() {
    this.modalService.openCreateLocation(null); // null = создание в корне
    this.isCreateMenuOpen.set(false);
  }

  changeLang(lang: string) {
    this.loc.setLanguage(lang);
    this.isLangMenuOpen.set(false);
  }

  toggleTheme() { this.themeService.toggleTheme(); }
  toggleMenu() { this.isMenuOpen.set(!this.isMenuOpen()); }
  closeMenu() { this.isMenuOpen.set(false); }

  languages = [
  { code: 'en', label: 'English', flag: '🇺🇸' },
  { code: 'ru', label: 'Русский', flag: '🇷🇺' },
  { code: 'uk', label: 'Українська', flag: '🇺🇦' }
];
}