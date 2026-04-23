import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { tap, finalize, catchError, switchMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface AppUser {
  id: string;
  email: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/auth`;
  private userSubject = new BehaviorSubject<AppUser | null | undefined>(undefined);
  user$ = this.userSubject.asObservable();

  // ================= АВТОРИЗАЦИЯ =================
  checkAuth(): Observable<AppUser | null> {
    return this.http.get<AppUser>(`${this.apiUrl}/me`).pipe(
      tap(user => this.userSubject.next(user)),
      catchError(() => {
        this.userSubject.next(null);
        return of(null);
      })
    );
  }

  login(email: string, password: string): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        switchMap(() => this.checkAuth())
      );
  }

  refreshToken(): Observable<any> {
  return this.http.post<any>(`${this.apiUrl}/refresh`, {}).pipe(
    catchError(err => {
      this.userSubject.next(null);
      return throwError(() => err);
    })
  );
}

  register(dto: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, dto);
  }

  confirmRegistration(email: string, code: string): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/confirm-register`, { email, code })
      .pipe(switchMap(() => this.checkAuth()));
  }

  googleLogin(idToken: string): Observable<any> {
    return this.http
      .post<any>(`${this.apiUrl}/google-login`, { idToken })
      .pipe(switchMap(() => this.checkAuth()));
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout`, {}).pipe(
      finalize(() => this.userSubject.next(null)),
      catchError(() => {
        this.userSubject.next(null);
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
}