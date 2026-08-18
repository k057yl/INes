import { Component, inject, signal, HostListener, ElementRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../features/auth/services/auth.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { ThemeService } from '../../../core/services/theme.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnDestroy {
  private el = inject(ElementRef);
  public authService = inject(AuthService);
  public loc = inject(LocalizationService);
  public themeService = inject(ThemeService);
  private router = inject(Router);

  public isMobileMenuOpen = false;
  isLangMenuOpen = signal(false);

  user$ = this.authService.user$;
  private sub = new Subscription();

  get currentLang() { return this.loc.currentLang; }

  constructor() {
    this.sub.add(
      this.router.events.subscribe(() => {
        this.isMobileMenuOpen = false;
      })
    );
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  isAdmin(user: any): boolean {
    if (!user || !user.roles) return false;

    if (Array.isArray(user.roles)) {
      return user.roles.some((r: string) => r.toLowerCase() === 'inest_admin' || r.toLowerCase() === 'admin');
    }

    if (typeof user.roles === 'string') {
      return user.roles.toLowerCase() === 'inest_admin' || user.roles.toLowerCase() === 'admin';
    }

    return false;
  }

  onLogout() {
    this.authService.logout().subscribe({
      next: () => this.handleLogoutRedirect(),
      error: () => this.handleLogoutRedirect() 
    });
  }

  private handleLogoutRedirect() {
    this.router.navigate(['/login']);
  }

  toggleLangMenu(event: MouseEvent) {
    event.stopPropagation();
    this.isLangMenuOpen.set(!this.isLangMenuOpen());
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!this.el.nativeElement.contains(target)) {
      this.isLangMenuOpen.set(false);
      this.isMobileMenuOpen = false;
    }
  }

  changeLang(lang: string) {
    this.loc.setLanguage(lang);
    this.isLangMenuOpen.set(false);
  }

  toggleTheme() { 
    this.themeService.toggleTheme(); 
  }

  languages = [
    { code: 'en', label: 'English', flag: '🇺🇸' },
    { code: 'ru', label: 'Русский', flag: '🇷🇺' },
    { code: 'uk', label: 'Українська', flag: '🇺🇦' }
  ];
}