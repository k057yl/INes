import { Component, inject, OnInit, Input} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';

import { ItemService } from '../../../../shared/services/item.service';
import { LocationService } from '../../../../shared/services/location.service';
import { CategoryService } from '../../../../shared/services/category.service';
import { LendingService } from '../../../../shared/services/lending.service';
import { AuthService } from '../../../../core/services/auth.service';
import { LocalizationService } from '../../../../shared/services/localization.service';
import { DashboardModalService } from '../../../../features/dashboard/dashboard.modal.service';
import { StatusNamePipe } from '../../../../shared/pipe/status-name.pipe';

import { ITEM_STATUS_OPTIONS } from '../../../../models/constants/item-status.constants';
import { Item } from '../../../../models/entities/item.entity';

import { take, filter } from 'rxjs/operators';
import { FormErrorService } from '../../../../core/services/form-error.service';
import { ToastrService } from 'ngx-toastr';

interface PhotoSlot {
  file?: File;
  preview: string;
  isMain: boolean;
}

@Component({
  selector: 'app-item-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, RouterModule, StatusNamePipe],
  templateUrl: './item-form-modal.component.html',
  styleUrl: './item-form-modal.component.scss'
})
export class ItemFormModalComponent implements OnInit {
  @Input() item: Item | null = null; 
  @Input() parentId: string | null = null; 
  
  private fb = inject(FormBuilder);
  private itemService = inject(ItemService);
  private locationService = inject(LocationService);
  private categoryService = inject(CategoryService);
  private lendingService = inject(LendingService);
  private authService = inject(AuthService);
  private localizationService = inject(LocalizationService);
  private modalService = inject(DashboardModalService);
  private formErrorService = inject(FormErrorService);
  private toastr = inject(ToastrService);
  private translateService = inject(TranslateService);

  locations: any[] = [];
  categories: any[] = [];
  selectedPhotos: PhotoSlot[] = [];
  isLocationPredefined = false;
  
  readonly MAX_PHOTOS = 5;
  todayMax = new Date().toISOString().split('T')[0];
  readonly statusOptions = ITEM_STATUS_OPTIONS;

  form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    description: [''],
    categoryId: ['', Validators.required],
    storageLocationId: ['', Validators.required],
    status: [0, Validators.required],
    currency: ['USD', Validators.required],
    purchaseDate: [this.todayMax, [this.dateNotInFutureValidator]],
    purchasePrice: [null as number | null, [Validators.min(0)]],
    estimatedValue: [null as number | null, [Validators.min(0)]],
    personName: [''],
    contactEmail: ['', [Validators.email]],
    expectedReturnDate: [null as string | null],
    sendNotification: [false],
    addPhoto: [false]
  });

  get isEdit(): boolean { return !!this.item; }
  get isLendingStatus(): boolean {
    const s = Number(this.form.get('status')?.value);
    return s === 1 || s === 7;
  }

  ngOnInit() {
    this.loadInitialData();

    this.form.get('status')?.valueChanges.subscribe(val => this.applyLendingLogic(val));

    if (this.isEdit && this.item) {
      this.patchFormValues(this.item);
    } else {
      this.setupNewItemDefaults();
    }

    this.applyLendingLogic(this.form.get('status')?.value);
  }

  private applyLendingLogic(statusId: any) {
    const s = Number(statusId);
    const emailControl = this.form.get('contactEmail');
    const isLending = (s === 1 || s === 7);

    if (isLending) {
      this.authService.user$.pipe(
        filter(u => !!u && Object.keys(u).length > 0),
        take(1)
      ).subscribe(user => {
        
        const u = user as any;
        const foundEmail = u.email || 
                          u.Email || 
                          u.emailAddress ||
                          u.userName ||
                          u['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];

        if (foundEmail) {
          emailControl?.setValue(foundEmail, { emitEvent: false });
          emailControl?.disable({ emitEvent: false });
        } else {
          emailControl?.enable({ emitEvent: false });
        }
      });
    } else {
      emailControl?.enable({ emitEvent: false });
      if (s === 0) emailControl?.setValue('', { emitEvent: false });
    }

    this.updateLendingValidators(isLending);
  }

  private setupNewItemDefaults() {
    this.form.patchValue({ currency: this.localizationService.getDefaultCurrency() });
    if (this.parentId) {
      this.form.patchValue({ storageLocationId: this.parentId });
      this.isLocationPredefined = true;
    }
  }

  private patchFormValues(item: Item) {
    this.form.patchValue({
      name: item.name,
      description: item.description,
      categoryId: item.categoryId,
      storageLocationId: item.storageLocationId,
      status: item.status,
      currency: item.currency || 'USD',
      purchaseDate: item.purchaseDate ? item.purchaseDate.split('T')[0] : '',
      purchasePrice: item.purchasePrice,
      estimatedValue: item.estimatedValue
    });

    if (item.lending) {
      this.form.patchValue({
        personName: item.lending.personName,
        contactEmail: item.lending.contactEmail,
        expectedReturnDate: item.lending.expectedReturnDate?.split('T')[0],
        sendNotification: item.lending.sendNotification
      });
    }

    if (this.isEdit) {
      this.form.get('status')?.disable({ emitEvent: false });
    }

    if (item.status !== 0) {
      this.form.disable();
      this.form.get('addPhoto')?.enable();
    }
  }

  private loadInitialData() {
    this.locationService.getAll().subscribe(res => this.locations = res);
    this.categoryService.getAll().subscribe(res => {
      this.categories = res.sort((a: any, b: any) => a.name.localeCompare(b.name));
    });
  }

  private updateLendingValidators(isRequired: boolean) {
    const fields = ['personName', 'expectedReturnDate'];
    fields.forEach(f => {
      const c = this.form.get(f);
      isRequired ? c?.setValidators([Validators.required]) : (c?.clearValidators(), c?.setValue(null));
      c?.updateValueAndValidity();
    });
  }

  private dateNotInFutureValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    return control.value > new Date().toISOString().split('T')[0] ? { futureDate: true } : null;
  }

  setMainPhoto(index: number): void {
    this.selectedPhotos.forEach((p, i) => p.isMain = (i === index));
  }

  removePhoto(index: number, event?: Event): void {
    if (event) event.stopPropagation();
    const removedWasMain = this.selectedPhotos[index].isMain;
    this.selectedPhotos.splice(index, 1);
    if (removedWasMain && this.selectedPhotos.length > 0) this.selectedPhotos[0].isMain = true;
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const val = this.form.getRawValue();
    const formData = new FormData();

    formData.append('name', val.name!);
    formData.append('description', val.description || '');
    formData.append('categoryId', val.categoryId!);
    formData.append('storageLocationId', val.storageLocationId!);
    formData.append('status', val.status!.toString());
    formData.append('currency', val.currency!);

    if (val.purchaseDate) {
      formData.append('purchaseDate', new Date(val.purchaseDate).toISOString());
    }

    if (val.purchasePrice != null) {
      formData.append('purchasePrice', val.purchasePrice.toString());
    }

    const estValue = val.estimatedValue ?? val.purchasePrice;
    if (estValue != null) {
      formData.append('estimatedValue', estValue.toString());
    }

    if (this.isLendingStatus) {
      formData.append('personName', val.personName || '');
      formData.append('contactEmail', val.contactEmail || '');
      if (val.expectedReturnDate) {
        formData.append('expectedReturnDate', new Date(val.expectedReturnDate).toISOString());
      }
      formData.append('sendNotification', (!!val.sendNotification).toString());
    }

    if ((!this.isEdit || val.addPhoto) && this.selectedPhotos.length > 0) {
      this.selectedPhotos.forEach(p => {
        if (p.file) {
          formData.append('photos', p.file);
          if (p.isMain) {
            formData.append('mainPhotoName', p.file.name);
          }
        }
      });
    }

    const request$: Observable<any> = this.isEdit 
      ? this.itemService.update(this.item!.id, formData) 
      : this.itemService.createWithPhoto(formData);

    request$.subscribe({
      next: (res: any) => {
        const successKey = res?.message || (this.isEdit ? 'ITEMS.SUCCESS.UPDATE' : 'ITEMS.SUCCESS.CREATE');

        const translatedMsg = this.translateService.instant(successKey);

        this.toastr.success(translatedMsg);
        this.modalService.confirm(res);
      },
      error: (err: any) => {
        if (err.details) {
          this.formErrorService.mapServerErrorsToForm(this.form, err.details);
        }
        console.error('Ошибка сохранения:', err);
      }
    });
  }

  private async compressImage(file: File, maxWidth: number, quality: number): Promise<{ file: File, preview: string }> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      
      reader.onload = (event) => {
        const img = new Image();
        img.src = event.target?.result as string;
        
        img.onload = () => {
          const canvas = document.createElement('canvas');
          let width = img.width;
          let height = img.height;

          if (width > maxWidth) {
            height = Math.round((height * maxWidth) / width);
            width = maxWidth;
          }

          canvas.width = width;
          canvas.height = height;
          const ctx = canvas.getContext('2d');
          if (!ctx) return reject('Canvas context is null');
          
          ctx.drawImage(img, 0, 0, width, height);
          const preview = canvas.toDataURL('image/jpeg', quality);

          canvas.toBlob((blob) => {
            if (blob) {
              const newFileName = file.name.replace(/\.[^/.]+$/, ".jpg");
              const compressedFile = new File([blob], newFileName, {
                type: 'image/jpeg',
                lastModified: Date.now()
              });
              resolve({ file: compressedFile, preview });
            } else {
              reject('Blob creation failed');
            }
          }, 'image/jpeg', quality);
        };
      };
    });
  }

  async onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const filesToProcess = Array.from(input.files).slice(0, this.MAX_PHOTOS - this.selectedPhotos.length);

    for (const file of filesToProcess) {
      try {
        const compressed = await this.compressImage(file, 1024, 0.75);
        this.selectedPhotos.push({ 
          file: compressed.file, 
          preview: compressed.preview, 
          isMain: this.selectedPhotos.length === 0 
        });
      } catch (err) {
        console.error('Ошибка при обработке фото:', err);
      }
    }
    input.value = '';
  }

  isControlInvalid(name: string): boolean {
    const c = this.form.get(name);
    return !!(c && (c.touched || c.errors?.['serverError']) && c.invalid);
  }

  returnItem() {
    if (!this.item) return;
    this.lendingService.returnItem(this.item.id, { returnedDate: new Date().toISOString() }).subscribe(() => this.modalService.confirm());
  }

  cancel(): void { this.modalService.close(); }
}