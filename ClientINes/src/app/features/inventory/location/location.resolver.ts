import { inject } from '@angular/core';
import { ResolveFn, Router } from '@angular/router';
import { LocationService } from '../../../shared/services/location.service';
import { StorageLocation } from '../../../models/entities/storage-location.entity';
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