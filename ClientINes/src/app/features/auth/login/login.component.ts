import { Component, inject, OnInit, AfterViewInit, NgZone } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';

declare var google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  public themeService = inject(ThemeService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private ngZone = inject(NgZone);

  email = '';
  password = '';
  error?: string;
  showGoogle = false;

  ngOnInit() {
    this.resetForm();
  }

  toggleGoogle() {
    this.showGoogle = !this.showGoogle;
    if (this.showGoogle) {
      setTimeout(() => this.initGoogleButton(), 50);
    }
  }

  private resetForm() {
    this.email = '';
    this.password = '';
    this.error = undefined;
  }

  private initGoogleButton() {
    const clientId = '477633498097-9ovq0psecdt4iu5lhh631dfjofgdlt2e.apps.googleusercontent.com';

    const renderAction = () => {
      if (typeof google !== 'undefined' && google.accounts) {
        google.accounts.id.initialize({
          client_id: clientId,
          callback: (response: any) => this.handleGoogleLogin(response),
          auto_select: false
        });

        const btnContainer = document.getElementById('googleBtn');
        if (btnContainer) {
          btnContainer.innerHTML = ''; 
          google.accounts.id.renderButton(btnContainer, { 
            theme: 'outline', 
            size: 'large', 
            width: 370
          });
        }
      } else {
        setTimeout(renderAction, 100);
      }
    };

    setTimeout(renderAction, 50);
  }

  private handleGoogleLogin(response: any) {
    this.ngZone.run(() => {
      this.authService.googleLogin(response.credential).subscribe({
        next: () => this.router.navigate(['/main']),
        error: (err) => {
          this.error = "ERRORS.GOOGLE_FAILED";
          console.error('Google Auth Error:', err);
        }
      });
    });
  }

  login() {
    this.error = undefined;
    this.authService.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(["/main"]),
      error: (err) => this.error = err.error?.error || "ERRORS.DEFAULT"
    });
  }
}