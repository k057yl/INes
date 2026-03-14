import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LocationService } from '../../../../shared/services/location.service';
import { FeatureService } from '../../../../core/services/feature.service';
import { CreateLocationDto } from '../../../../models/dtos/location.dto';

@Component({
  selector: 'app-location-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './location-create.component.html',
  styleUrl: './location-create.component.scss'
})
export class LocationCreateComponent {
  private fb = inject(FormBuilder);
  private locationService = inject(LocationService);
  private router = inject(Router);
  public featureService = inject(FeatureService);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    color: ['#00f5d4'],
    sortOrder: [0],
    isSalesLocation: [false],
    isLendingLocation: [false]
  });

  onSubmit() {
    if (this.form.valid) {
      const rawValue = this.form.getRawValue();

      this.locationService.create(rawValue as unknown as CreateLocationDto).subscribe({
        next: () => this.router.navigate(['/main']),
        error: (err) => console.error('Ошибка сохранения:', err)
      });
    }
  }

  cancel() {
    this.router.navigate(['/main']);
  }
}