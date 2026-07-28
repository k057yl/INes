import { inject } from '@angular/core';
import { ResolveFn, Router } from '@angular/router';
import { LocationService } from '../../../core/services/location.service';
import { catchError, EMPTY, forkJoin, map } from 'rxjs';

export const locationResolver: ResolveFn<any> = (route) => {
  const locationService = inject(LocationService);
  const router = inject(Router);
  const id = route.paramMap.get('id');

  if (!id) {
    router.navigate(['/dashboard']);
    return EMPTY;
  }

  return forkJoin({
    header: locationService.getLocationHeader(id),
    items: locationService.getLocationItems(id),
    children: locationService.getLocationChildren(id)
  }).pipe(
    map(({ header, items, children }) => ({
      ...header,
      items: items || [],
      children: children || []
    })),
    catchError(() => {
      router.navigate(['/dashboard']);
      return EMPTY;
    })
  );
};