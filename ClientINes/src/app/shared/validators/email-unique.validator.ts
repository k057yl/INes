import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, catchError, first } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';

export function emailUniqueValidator(authService: AuthService): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value) return of(null);

    return timer(500).pipe(
      switchMap(() => authService.checkEmailUnique(control.value)),
      map(isUnique => (isUnique ? null : { emailExists: true })),
      catchError(() => of(null)),
      first()
    );
  };
}