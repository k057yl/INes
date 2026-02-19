import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-confirm-register',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './confirm-register.component.html',
  styleUrl: './register.component.css'
})
export class ConfirmRegisterComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  email = '';
  password = '';
  code = '';
  message?: string;
  error?: string;

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      this.password = params['password'] || '';
    });
  }

  confirm() {
    this.error = undefined;
    this.message = undefined;

    this.http.post(`${environment.apiBaseUrl}/auth/confirm-register`, {
      email: this.email,
      password: this.password,
      code: this.code
    }).subscribe({
      next: () => {
        this.message = 'AUTH.CONFIRM.SUCCESS_MSG';
        setTimeout(() => this.router.navigate(['/main']), 1500);
      },
      error: err => this.error = err.error?.error || 'AUTH.ERRORS.CONFIRM_FAILED'
    });
  }
}