import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  public themeService = inject(ThemeService);
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  error?: string;

  login() {
    this.error = undefined;
    
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.router.navigate(["/main"]);
      },
      error: (err) => {
        this.error = err.error?.error || "ERRORS.DEFAULT";
      }
    });
  }
}