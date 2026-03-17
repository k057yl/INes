import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LocationService } from '../../../../shared/services/location.service';
import { FeatureService } from '../../../../core/services/feature.service';
import { CreateLocationDto } from '../../../../models/dtos/location.dto';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-location-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './location-create.component.html',
  styleUrl: './location-create.component.scss'
})
export class LocationCreateComponent {
  private fb = inject(FormBuilder);
  private locationService = inject(LocationService);
  private router = inject(Router);
  public featureService = inject(FeatureService);

  public readonly presetColors = ['var(--g-blue)', 'var(--g-red)', 'var(--g-yellow)', 'var(--g-green)'];

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    color: [''],
    sortOrder: [0],
    isSalesLocation: [false],
    isLendingLocation: [false]
  });

  selectPresetColor(color: string) {
    this.form.patchValue({ color });
  }

  onSubmit() {
    if (this.form.valid) {
      const rawValue = this.form.getRawValue();

      if (!rawValue.color) {
        const randomIndex = Math.floor(Math.random() * this.presetColors.length);
        rawValue.color = this.presetColors[randomIndex];
      }

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