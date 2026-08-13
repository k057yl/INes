import { Component, Input, inject, OnInit } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ColorChromeModule } from 'ngx-color/chrome';
import { ToastrService } from 'ngx-toastr';

import { LocationService } from '../../services/location.service';
import { FeatureService } from '../../../../core/services/feature.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { StorageLocation } from '../../contracts/storage-location';
import { AuthService } from '../../../auth/services/auth.service';
import { TutorialService, TutorialStep } from '../../../../core/services/tutorial.service';
import { take } from 'rxjs/operators';

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
  private translate = inject(TranslateService);
  private toastr = inject(ToastrService);
  public featureService = inject(FeatureService);

  private authService = inject(AuthService);
  private tutorialService = inject(TutorialService);

  public readonly presetColors = ['var(--g-blue)', 'var(--g-red)', 'var(--g-yellow)', 'var(--g-green)'];

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
    console.log('Input parentId:', this.parentId);
    console.log('Form before:', this.form.getRawValue());
    if (!this.isEdit) {
      this.checkAndStartTutorial();
    }
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
    console.log('Form after:', this.form.getRawValue());
  }

  private checkAndStartTutorial() {
    this.authService.user$.pipe(take(1)).subscribe(user => {
      if (!user) return;

      const isFormPassed = (user.completedTutorials & TutorialStep.LocationForm) === TutorialStep.LocationForm;

      if (!isFormPassed) {
        setTimeout(() => {
          this.tutorialService.startLocationFormTour(() => {
            user.completedTutorials |= TutorialStep.LocationForm;
            this.authService.updateLocalUserTutorial(TutorialStep.LocationForm);
          });
        }, 400);
      }
    });
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
      next: (res: any) => {
        this.toastr.success(this.translate.instant(this.isEdit ? 'LOCATIONS.SUCCESS.RENAME' : 'LOCATIONS.SUCCESS.CREATE'));
        this.modalService.confirm(res);
      },
      error: (err: any) => {
        this.toastr.error(this.translate.instant('SYSTEM.DEFAULT_ERROR'));
      }
    });
  }

  cancel() {
    this.modalService.close();
  }
}