import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { tap, finalize, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface AppUser {
  id: string;
  email: string;
  roles: string[];
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/auth`;
  
  private readonly TOKEN_KEY = 'jwt';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  private tokenSubject = new BehaviorSubject<string | null>(localStorage.getItem('jwt'));
  token$ = this.tokenSubject.asObservable();

  private userSubject = new BehaviorSubject<AppUser | null>(null);
  user$ = this.userSubject.asObservable();

  constructor() {
    const token = this.tokenSubject.value;
    if (token) {
      this.userSubject.next(this.parseUser(token));
    }
  }

  // ================= AUTH METHODS =================

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(tap(res => this.setSession(res)));
  }

  refreshToken(): Observable<AuthResponse> {
    const accessToken = localStorage.getItem(this.TOKEN_KEY);
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);

    return this.http
      .post<AuthResponse>(`${this.apiUrl}/refresh`, { accessToken, refreshToken })
      .pipe(
        tap(res => this.setSession(res)),
        catchError(err => {
          this.logout().subscribe();
          return throwError(() => err);
        })
      );
  }

  register(dto: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, dto);
  }

  confirmRegistration(email: string, code: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/confirm-register`, { email, code })
      .pipe(tap(res => this.setSession(res)));
  }

  googleLogin(idToken: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/google-login`, { idToken })
      .pipe(tap(res => this.setSession(res)));
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

  // ================= VALIDATION =================

  checkEmailUnique(email: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/check-email`, {
      params: { email }
    }).pipe(
      catchError(() => of(true))
    );
  }

  // ================= SESSION MANAGEMENT =================

  public setSession(res: AuthResponse) {
    if (!res.token) return;
    localStorage.setItem(this.TOKEN_KEY, res.token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, res.refreshToken);
    
    this.tokenSubject.next(res.token);
    this.userSubject.next(this.parseUser(res.token));
  }

  private clearLocalSession() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    this.tokenSubject.next(null);
    this.userSubject.next(null);
  }

  private restoreSession() {
    const token = localStorage.getItem(this.TOKEN_KEY);
    if (token) {
      this.tokenSubject.next(token);
      this.userSubject.next(this.parseUser(token));
    } else {
      this.clearLocalSession();
    }
  }

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

  isLoggedIn(): boolean {
    return !!this.tokenSubject.value; 
  }

  isAuthenticated(): boolean {
    return this.isLoggedIn();
  }
}