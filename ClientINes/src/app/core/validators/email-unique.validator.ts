import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { inject } from '@angular/core';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, catchError, first } from 'rxjs/operators';
import { AuthService } from '../../features/auth/services/auth.service';

export function emailUniqueValidator(authService = inject(AuthService)): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value) return of(null);

    return timer(400).pipe(
      switchMap(() => authService.checkEmailUnique(control.value)),
      map(response => {
        const isUnique = typeof response === 'boolean' ? response : (response as any)?.isUnique;
        
        return isUnique ? null : { emailExists: true };
      }),
      catchError(err => {
        console.error('Email uniqueness check error:', err);
        return of(null);
      }),
      first()
    );
  };
}