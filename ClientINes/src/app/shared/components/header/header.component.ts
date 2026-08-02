import { Component, inject, signal, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { ThemeService } from '../../../core/services/theme.service';
import { TranslateModule } from '@ngx-translate/core';
import { DashboardModalService } from '../../../features/dashboard/dashboard.modal.service';
import { LocationService } from '../../../core/services/location.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  private el = inject(ElementRef);
  public authService = inject(AuthService);
  public loc = inject(LocalizationService);
  public themeService = inject(ThemeService);
  private router = inject(Router);
  public modalService = inject(DashboardModalService);
  private locationService = inject(LocationService);
  
  public isMobileMenuOpen = false;

  isLangMenuOpen = signal(false);
  isCreateMenuOpen = signal(false);
  hasLocations = signal<boolean>(false);
  

  user$ = this.authService.user$;

  get currentLang() { return this.loc.currentLang; }

  isAdmin(user: any): boolean {
    return user?.roles?.includes('inest_admin') ?? false;
  }

  ngOnInit() {
    this.checkLocations();
    this.modalService.refreshData$.subscribe(() => this.checkLocations());
  }

  private checkLocations() {
    this.locationService.getTree().subscribe(locs => {
      this.hasLocations.set(locs && locs.length > 0);
    });
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

  toggleCreateMenu(event: MouseEvent) {
    event.stopPropagation();
    this.isLangMenuOpen.set(false);
    this.isCreateMenuOpen.set(!this.isCreateMenuOpen());
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!this.el.nativeElement.contains(target)) {
      this.isLangMenuOpen.set(false);
      this.isCreateMenuOpen.set(false);
    }
  }

  openCreateItem() {
    this.modalService.openItemForm(); 
    this.isCreateMenuOpen.set(false);
  }

  openCreateLocation() {
    this.modalService.openLocationForm(); 
    this.isCreateMenuOpen.set(false);
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