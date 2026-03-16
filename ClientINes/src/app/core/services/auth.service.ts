import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, finalize, catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface AppUser {
  id: string;
  email: string;
  roles: string[];
}

export interface AuthResponse {
  token: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/auth`;
  private readonly TOKEN_KEY = 'jwt';

  private tokenSubject = new BehaviorSubject<string | null>(null);
  token$ = this.tokenSubject.asObservable();

  private userSubject = new BehaviorSubject<AppUser | null>(null);
  user$ = this.userSubject.asObservable();

  constructor() {
    this.restoreSession();
  }

  // ================= AUTH METHODS =================

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap(res => this.setSession(res.token))
      );
  }

  register(dto: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, dto);
  }

  confirmRegistration(email: string, code: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/confirm-register`, { email, code })
      .pipe(
        tap(res => this.setSession(res.token))
      );
  }

  googleLogin(idToken: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/google-login`, { idToken })
      .pipe(
        tap(res => this.setSession(res.token))
      );
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout`, {}).pipe(
      finalize(() => this.clearLocalSession()),
      catchError(() => {
        this.clearLocalSession();
        return of(null);
      })
    );
  }

  // ================= SESSION MANAGEMENT =================

  public setSession(token: string) {
    if (!token) return;
    localStorage.setItem(this.TOKEN_KEY, token);
    this.tokenSubject.next(token);
    this.userSubject.next(this.parseUser(token));
  }

  private clearLocalSession() {
    localStorage.removeItem(this.TOKEN_KEY);
    this.tokenSubject.next(null);
    this.userSubject.next(null);
  }

  private restoreSession() {
    const token = localStorage.getItem(this.TOKEN_KEY);
    if (token && !this.isTokenExpired(token)) {
      this.tokenSubject.next(token);
      this.userSubject.next(this.parseUser(token));
    } else {
      this.clearLocalSession();
    }
  }

  // ================= JWT PARSE =================

  private parseUser(token: string): AppUser {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        id: payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '',
        email: payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || '',
        roles: payload.roles || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || []
      };
    } catch {
      return { id: '', email: '', roles: [] };
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp ? Date.now() >= payload.exp * 1000 : false;
    } catch { return true; }
  }

  // ================= HELPERS =================

  isAuthenticated(): boolean {
    return !!this.tokenSubject.value;
  }

  checkEmailUnique(email: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/check-email`, {
      params: { email }
    }).pipe(
      catchError(() => of(true))
    );
  }
}