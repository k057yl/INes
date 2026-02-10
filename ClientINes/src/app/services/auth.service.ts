import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface AppUser {
  id: string;
  email: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {

  private apiUrl = `${environment.apiBaseUrl}/auth`;

  private tokenSubject = new BehaviorSubject<string | null>(null);
  token$ = this.tokenSubject.asObservable();

  private userSubject = new BehaviorSubject<AppUser | null>(null);
  user$ = this.userSubject.asObservable();

  private pendingEmailKey = 'pending_email';

  constructor(private http: HttpClient) {
    this.restoreSession();
  }

  // ================= REGISTER =================

  register(email: string, username: string, password: string) {
    localStorage.setItem(this.pendingEmailKey, email);

    return this.http.post(`${this.apiUrl}/register`, {
      email,
      username,
      password
    });
  }

  confirmEmail(code: string) {
    const email = localStorage.getItem(this.pendingEmailKey);

    return this.http.post(`${this.apiUrl}/confirm-email`, {
      email,
      code
    });
  }

  // ================= LOGIN =================

  login(email: string, password: string) {
    return this.http
      .post<{ token: string }>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap(res => this.setSession(res.token))
      );
  }

  logout() {
    localStorage.removeItem('jwt');
    this.tokenSubject.next(null);
    this.userSubject.next(null);
  }

  // ================= SESSION =================

  private setSession(token: string) {
    localStorage.setItem('jwt', token);
    this.tokenSubject.next(token);
    this.userSubject.next(this.parseUser(token));
  }

  private restoreSession() {
    const token = localStorage.getItem('jwt');
    if (!token) return;

    if (this.isTokenExpired(token)) {
      this.logout();
      return;
    }

    this.tokenSubject.next(token);
    this.userSubject.next(this.parseUser(token));
  }

  getToken(): string | null {
    return localStorage.getItem('jwt');
  }

  // ================= JWT PARSE =================

  private parseUser(token: string): AppUser {
    const payload = JSON.parse(atob(token.split('.')[1]));

    return {
      id: payload.sub ?? '',
      email: payload.email ?? '',
      roles: payload.roles ?? []
    };
  }

  private isTokenExpired(token: string): boolean {
    const payload = JSON.parse(atob(token.split('.')[1]));
    if (!payload.exp) return false;

    return Date.now() >= payload.exp * 1000;
  }

  // ================= HELPERS =================

  isAuthenticated(): boolean {
    return !!this.tokenSubject.value;
  }
}