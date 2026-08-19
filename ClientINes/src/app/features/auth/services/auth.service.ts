import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { tap, finalize, catchError, switchMap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { AppUser } from '../dtos/create-user';
import { TutorialStep } from '../../../core/services/tutorial.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/auth`;
  private userSubject = new BehaviorSubject<AppUser | null | undefined>(undefined);
  user$ = this.userSubject.asObservable();

  // ================= АВТОРИЗАЦИЯ =================
  checkAuth(explicitToken?: string): Observable<AppUser | null> {
    const token = explicitToken || localStorage.getItem('token');
    
    if (!token) {
      this.clearLocalSession();
      return of(null);
    }

    const headers = {
      'Authorization': `Bearer ${token}`
    };

    return this.http.get<AppUser>(`${this.apiUrl}/me`, { headers }).pipe(
      tap(user => this.userSubject.next(user)),
      catchError(() => {
        this.clearLocalSession();
        return of(null);
      })
    );
  }

  login(email: string, password: string): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        switchMap((response) => {
          const token =
            response?.data?.token ||
            response?.token ||
            response?.data?.accessToken ||
            response?.accessToken;

          if (token) {
            localStorage.setItem('token', token);
            return this.checkAuth(token);
          }
          return this.checkAuth();
        })
      );
  }

  refreshToken(): Observable<any> {
    const currentToken = localStorage.getItem('token');

    return this.http.post<any>(
      `${this.apiUrl}/refresh`, 
      { accessToken: currentToken }, 
      { withCredentials: true }
    ).pipe();
  }

  resendCode(data: { email: string }) {
    return this.http.post(`${this.apiUrl}/resend-code`, data);
  }

  register(dto: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, dto);
  }

  confirmRegistration(email: string, code: string): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/confirm-register`, { email, code })
      .pipe(
        switchMap((response) => {
          const token = response?.data?.token || response?.token || response?.data?.accessToken || response?.accessToken;
          if (token) {
            localStorage.setItem('token', token);
            return this.checkAuth(token);
          }
          return this.checkAuth();
        })
      );
  }

  googleLogin(idToken: string): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/google-login`, { idToken })
      .pipe(
        switchMap((response) => {
          const token = response?.data?.token || response?.token || response?.data?.accessToken || response?.accessToken;
          if (token) {
            localStorage.setItem('token', token);
            return this.checkAuth(token);
          }
          return this.checkAuth();
        })
      );
  }

  forgotPassword(data: { email: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, data);
  }

  resetPassword(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, dto);
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

  // ================= ВАЛИДАЦИЯ =================

  checkEmailUnique(email: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/check-email`, {
      params: { email }
    }).pipe(
      catchError(() => of(true))
    );
  }

  // ================= ПУБЛИЧНЫЕ МЕТОДЫ =================

  isLoggedIn(): boolean {
    return !!this.userSubject.value; 
  }

  isAuthenticated(): boolean {
    return this.isLoggedIn();
  }

  // ================= СБРОС СЕССИИ БЕЗ СЕТИ =================

  clearLocalSession(): void {
    localStorage.removeItem('token');
    this.userSubject.next(null);
  }

  // ================= ТУТОРИАЛ =================

  public updateLocalUserTutorial(step: TutorialStep) {
    const currentUser = this.userSubject.value;
    if (currentUser) {
      currentUser.completedTutorials |= step;
      this.userSubject.next({ ...currentUser });
    }
  }

  deleteAccount(password?: string): Observable<any> {
    const payload = password ? { password } : {};
    return this.http.post<any>(`${this.apiUrl}/delete-account`, payload, {
      withCredentials: true
    }).pipe(
      tap(() => this.clearLocalSession())
    );
  }
}