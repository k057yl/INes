import { inject } from '@angular/core';
import { ResolveFn, Router } from '@angular/router';
import { LocationService } from '../../../core/services/location.service';
import { StorageLocation } from '../../../core/contracts/storage-location';
import { catchError, EMPTY } from 'rxjs';

export const locationResolver: ResolveFn<StorageLocation> = (route) => {
  const locationService = inject(LocationService);
  const router = inject(Router);
  const id = route.paramMap.get('id');

  if (!id) {
    router.navigate(['/dashboard']);
    return EMPTY;
  }

  return locationService.getById(id).pipe(
    catchError(() => {
      
      router.navigate(['/dashboard']);
      return EMPTY;
    })
  );
};