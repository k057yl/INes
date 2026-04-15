import { Component, Input, inject, OnInit } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ColorChromeModule } from 'ngx-color/chrome';

import { LocationService } from '../../../../shared/services/location.service';
import { FeatureService } from '../../../../core/services/feature.service';
import { DashboardModalService } from '../../../../features/dashboard/dashboard.modal.service';
import { StorageLocation } from '../../../../models/entities/storage-location.entity';
import { CreateLocationDto } from '../../../../models/dtos/location.dto';

@Component({
  selector: 'app-location-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, ColorChromeModule],
  templateUrl: './location-form-modal.component.html',
  styleUrl: './location-form-modal.component.scss'
})
export class LocationFormModalComponent implements OnInit {
  @Input() location: StorageLocation | null = null; 
  @Input() parentId: string | null = null; 

  private fb = inject(FormBuilder);
  private locationService = inject(LocationService);
  private modalService = inject(DashboardModalService);
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
    parentLocationId: [null as string | null],
    isSalesLocation: [false],
    isLendingLocation: [false]
  });

  get isEdit(): boolean { return !!this.location; }

  ngOnInit() {
    if (this.isEdit && this.location) {
      const loc = this.location as any;
      
      this.form.patchValue({
        name: loc.name,
        color: loc.color,
        parentLocationId: loc.parentLocationId || loc.parentId || loc.parentLocation?.id || null,
        isSalesLocation: !!loc.isSalesLocation,
        isLendingLocation: !!loc.isLendingLocation
      });
    } else if (this.parentId) {
      this.form.patchValue({ parentLocationId: this.parentId });
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
    if (this.form.invalid) return;

    const rawValue = this.form.getRawValue();

    if (!rawValue.color) {
      rawValue.color = this.presetColors[Math.floor(Math.random() * this.presetColors.length)];
    }

    const request$ = this.isEdit && this.location
      ? (this.locationService as any).update(this.location.id, rawValue) 
      : this.locationService.create(rawValue as any);

    request$.subscribe({
      next: (res: any) => this.modalService.confirm(res),
      error: (err: any) => console.error('Ошибка при сохранении локации:', err)
    });
  }

  cancel() {
    this.modalService.close();
  }
}