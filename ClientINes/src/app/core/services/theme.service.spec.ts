import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  let service: ThemeService;

  beforeEach(() => {
    localStorage.clear();
    document.body.className = '';
    TestBed.configureTestingModule({
      providers: [ThemeService]
    });
    service = TestBed.inject(ThemeService);
  });

  it('toggleTheme() должен переключать тему и добавлять класс на body', () => {
    expect(service.isDarkTheme()).toBeFalse();

    service.toggleTheme();

    expect(service.isDarkTheme()).toBeTrue();
    expect(localStorage.getItem('theme')).toBe('dark');
    expect(document.body.classList.contains('dark-theme')).toBeTrue();
  });
});