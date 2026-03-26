import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { LocationService } from '../../../../shared/services/location.service';
import { FeatureService } from '../../../../core/services/feature.service';
import { CreateLocationDto } from '../../../../models/dtos/location.dto';
import { TranslateModule } from '@ngx-translate/core';
import { ColorChromeModule } from 'ngx-color/chrome';

@Component({
  selector: 'app-location-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ColorChromeModule
  ],
  templateUrl: './location-create.component.html',
  styleUrl: './location-create.component.scss'
})
export class LocationCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private locationService = inject(LocationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  public featureService = inject(FeatureService);

  public readonly presetColors = [
    'var(--g-blue)',
    'var(--g-red)',
    'var(--g-yellow)',
    'var(--g-green)'
  ];

  showColorPicker = false;
  tempColor = '#ffffff';

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    color: [''],
    sortOrder: [0],
    parentId: [null as string | null],
    isSalesLocation: [false],
    isLendingLocation: [false]
  });

  ngOnInit() {
    const parentId = this.route.snapshot.queryParamMap.get('parentId');
    if (parentId) {
      this.form.patchValue({ parentId });
    }
  }

  selectPresetColor(color: string) {
    this.form.patchValue({ color });
    this.showColorPicker = false;
  }

  openColorPicker() {
    const current = this.form.controls.color.value;
    this.tempColor = current && !current.startsWith('var') ? current : '#ffffff';
    this.showColorPicker = !this.showColorPicker;
  }

  confirmColor() {
    this.form.patchValue({ color: this.tempColor });
    this.showColorPicker = false;
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