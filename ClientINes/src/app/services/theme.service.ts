import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  isDarkTheme = signal(localStorage.getItem('theme') === 'dark');

  constructor() {
    this.applyTheme(this.isDarkTheme());
  }

  toggleTheme() {
    const newTheme = !this.isDarkTheme();
    this.isDarkTheme.set(newTheme);
    localStorage.setItem('theme', newTheme ? 'dark' : 'light');
    this.applyTheme(newTheme);
  }

  private applyTheme(isDark: boolean) {
    if (isDark) {
      document.body.classList.add('dark-theme');
    } else {
      document.body.classList.remove('dark-theme');
    }
  }
}