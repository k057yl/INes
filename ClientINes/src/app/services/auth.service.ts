import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, finalize, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { map } from 'rxjs/operators';

export interface AppUser {
  id: string;
  email: string;
  roles: string[];
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

  // ================= LOGIN / LOGOUT =================

  login(email: string, password: string): Observable<any> {
    return this.http
      .post<{ token: string | "unconfirmed" }>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap(res => {
          if (res.token === 'unconfirmed') throw { error: 'unconfirmed' };
          this.setSession(res.token);
        })
      );
  }

  googleLogin(idToken: string): Observable<any> {
    return this.http
      .post<{ token: string }>(`${this.apiUrl}/google-login`, { idToken })
      .pipe(
        tap(res => {
          this.setSession(res.token);
        })
      );
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout`, {}).pipe(
      finalize(() => {
        this.clearLocalSession();
      }),
      catchError(err => {
        console.error('Server logout failed, but local session cleared', err);
        return of(null);
      })
    );
  }

  // ================= SESSION MANAGEMENT =================

  private setSession(token: string) {
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
        email: payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload.sub || 'User',
        roles: payload.roles || []
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
    return this.http.get<{ isUnique: boolean }>(`${this.apiUrl}/check-email`, {
      params: { email }
    }).pipe(
      map(res => res.isUnique),
      catchError(() => of(true))
    );
  }
}