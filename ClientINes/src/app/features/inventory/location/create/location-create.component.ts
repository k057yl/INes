import { Component, Input, Output, EventEmitter, inject, OnInit } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LocationService } from '../../../../shared/services/location.service';
import { FeatureService } from '../../../../core/services/feature.service';
import { CreateLocationDto } from '../../../../models/dtos/location.dto';
import { TranslateModule } from '@ngx-translate/core';
import { ColorChromeModule } from 'ngx-color/chrome';
import { MainPageModalService } from '../../main/main-page.modal.service';

@Component({
  selector: 'app-create-location-modal',
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
  @Input() parentId: string | null = null; 
  
  @Output() close = new EventEmitter<void>();
  @Output() created = new EventEmitter<any>();

  private fb = inject(FormBuilder);
  private locationService = inject(LocationService);
  private modalService = inject(MainPageModalService);
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
    if (this.parentId) {
      this.form.patchValue({ parentId: this.parentId });
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
        next: (newLoc) => {
          this.modalService.confirm(newLoc); 
          this.created.emit(newLoc);
        },
        error: (err) => console.error('Ошибка сохранения:', err)
      });
    }
  }

  cancel() {
    this.modalService.close();
    this.close.emit();
  }
}