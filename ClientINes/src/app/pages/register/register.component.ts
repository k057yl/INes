import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TranslateModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private http = inject(HttpClient);
  private router = inject(Router);

  username = '';
  email = '';
  password = '';
  confirmPassword = '';
  message?: string;
  error?: string;

  register() {
    this.error = undefined;
    this.message = undefined;

    if (this.password !== this.confirmPassword) {
      this.error = 'AUTH.ERRORS.PASSWORD_MISMATCH';
      return;
    }

    this.http.post(`${environment.apiBaseUrl}/auth/register`, {
      username: this.username,
      email: this.email,
      password: this.password
    }).subscribe({
      next: () => {
        this.message = 'AUTH.REGISTER.SUCCESS_MSG';
        setTimeout(() => {
          this.router.navigate(['/confirm-register'], { 
            queryParams: { email: this.email, password: this.password } 
          });
        }, 1500);
      },
      error: err => this.error = err.error?.error || 'AUTH.ERRORS.REGISTER_FAILED'
    });
  }
}