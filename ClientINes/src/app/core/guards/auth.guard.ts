import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, take, filter } from 'rxjs/operators';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.user$.pipe(
    filter(user => user !== undefined),
    take(1),
    map(user => {
      if (user) return true;
      console.warn('AuthGuard: Вы не залогинены');
      return router.parseUrl('/login');
    })
  );
};

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.user$.pipe(
    filter(user => user !== undefined),
    take(1),
    map(user => {
      if (user) {
        return router.parseUrl('/dashboard');
      }
      return true;
    })
  );
};

export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.user$.pipe(
    filter(user => user !== undefined),
    take(1),
    map(user => {
      const isAdmin = user?.roles?.includes('inest_admin');
      if (isAdmin) {
        return true;
      }
      console.warn('AdminGuard: Куда лезешь? Тут только для админов');
      return router.parseUrl('/dashboard');
    })
  );
};