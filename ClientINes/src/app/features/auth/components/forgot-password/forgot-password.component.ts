import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TranslateModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  isLoading = false;
  message?: string;
  error?: string;

  onSubmit() {
    if (!this.email) return;

    this.isLoading = true;
    this.error = undefined;
    this.message = undefined;

    this.authService.forgotPassword({ email: this.email }).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/reset-password'], { 
          state: { email: this.email } 
        });
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        this.error = err.error?.error || 'SYSTEM.DEFAULT_ERROR';
      }
    });
  }
}