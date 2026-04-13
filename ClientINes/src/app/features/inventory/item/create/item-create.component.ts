import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ItemService } from '../../../../shared/services/item.service';
import { LocationService } from '../../../../shared/services/location.service';
import { CategoryService } from '../../../../shared/services/category.service';
import { TranslateModule } from '@ngx-translate/core';
import { ITEM_STATUS_OPTIONS } from '../../../../models/constants/item-status.constants';
import { AuthService } from '../../../../core/services/auth.service';
import { InestModalComponent } from '../../../../shared/components/modal/shared-modal/inest-modal.component';
import { LocalizationService } from '../../../../shared/services/localization.service';
import { TranslateService } from '@ngx-translate/core';

interface PhotoSlot {
  file: File;
  preview: string;
  isMain: boolean;
}

@Component({
  selector: 'app-item-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, RouterModule, InestModalComponent],
  templateUrl: './item-create.component.html',
  styleUrl: './item-create.component.scss'
})
export class ItemCreateComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  
  private itemService = inject(ItemService);
  private locationService = inject(LocationService);
  private categoryService = inject(CategoryService);
  private authService = inject(AuthService);
  private localizationService = inject(LocalizationService);
  private translate = inject(TranslateService);

  locations: any[] = [];
  categories: any[] = [];
  selectedPhotos: PhotoSlot[] = [];
  showCategoryModal = false;
  
  readonly MAX_PHOTOS = 5;
  todayMax = new Date().toISOString().split('T')[0];
  readonly statusOptions = ITEM_STATUS_OPTIONS;
  isLocationPredefined = false;

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
    addPhoto: [false],
    personName: [''],
    contactEmail: ['', [Validators.email]],
    expectedReturnDate: [null as string | null],
    sendNotification: [false]
  });

  get isLendingStatus(): boolean {
    const s = Number(this.form.get('status')?.value);
    return s === 1 || s === 7;
  }

  ngOnInit() {
  this.loadData();

    const initialCurrency = this.localizationService.getDefaultCurrency();
    this.form.patchValue({ currency: initialCurrency });

    this.translate.onLangChange.subscribe(() => {
      const currencyControl = this.form.get('currency');
      if (currencyControl?.pristine) {
        currencyControl.setValue(this.localizationService.getDefaultCurrency());
      }
    });

    this.route.queryParams.subscribe(params => {
      const locId = params['locationId'];
      if (locId) {
        this.form.patchValue({ storageLocationId: locId });
        this.isLocationPredefined = true;
      }
    });

    this.form.get('addPhoto')?.valueChanges.subscribe(val => {
      if (!val) this.selectedPhotos = [];
    });

    this.form.get('status')?.valueChanges.subscribe(statusId => {
      const s = Number(statusId);
      const isLending = (s === 1 || s === 7);
      const emailControl = this.form.get('contactEmail');

      if (s === 7) {
        this.authService.user$.subscribe(user => {
          if (user?.email) {
            emailControl?.setValue(user.email);
            emailControl?.disable(); 
          }
        });
      } else {
        emailControl?.enable();
        if (s !== 1) emailControl?.setValue(''); 
      }

      this.updateLendingValidators(isLending);
    });
  }

  private updateLendingValidators(isRequired: boolean) {
    const fields = ['personName', 'expectedReturnDate'];
    fields.forEach(fieldName => {
      const control = this.form.get(fieldName);
      if (isRequired) {
        control?.setValidators([Validators.required]);
      } else {
        control?.clearValidators();
        control?.setValue(null);
      }
      control?.updateValueAndValidity();
    });
  }

  private dateNotInFutureValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    const today = new Date().toISOString().split('T')[0];
    return control.value > today ? { futureDate: true } : null;
  }

  loadData() {
    this.locationService.getAll().subscribe(res => this.locations = res);
    this.categoryService.getAll().subscribe(res => {
      this.categories = res.sort((a: any, b: any) => a.name.localeCompare(b.name));
    });
  }

  onCategoryConfirmed(name: string) {
    this.categoryService.create({ name }).subscribe({
      next: (newCat) => {
        this.categories.push(newCat);
        this.categories.sort((a, b) => a.name.localeCompare(b.name));
        this.form.patchValue({ categoryId: newCat.id });
        this.showCategoryModal = false;
      },
      error: () => this.showCategoryModal = false 
    });
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
      formData.append('estimatedValue', (val.estimatedValue ?? val.purchasePrice).toString());
    }

    const s = Number(val.status);
    if (s === 1 || s === 7) {
      if (val.personName) formData.append('personName', val.personName);
      if (val.contactEmail) formData.append('contactEmail', val.contactEmail);
      if (val.expectedReturnDate) {
        formData.append('expectedReturnDate', new Date(val.expectedReturnDate).toISOString());
      }
      formData.append('sendNotification', (!!val.sendNotification).toString());
    }

    if (this.selectedPhotos.length > 0) {
      this.selectedPhotos.forEach(p => {
        formData.append('photos', p.file);
        if (p.isMain) formData.append('mainPhotoName', p.file.name);
      });
    }

    this.itemService.createWithPhoto(formData).subscribe({
      next: () => {
        const target = this.isLocationPredefined 
          ? ['/location', this.form.get('storageLocationId')?.value] 
          : ['/main'];
        this.router.navigate(target);
      },
      error: (err) => console.error('Error:', err)
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const files = Array.from(input.files);
    const availableSlots = this.MAX_PHOTOS - this.selectedPhotos.length;

    files.slice(0, availableSlots).forEach(file => {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        const isFirst = this.selectedPhotos.length === 0;
        this.selectedPhotos.push({ file, preview: e.target.result, isMain: isFirst });
      };
      reader.readAsDataURL(file);
    });
    input.value = ''; 
  }

  setMain(index: number) {
    this.selectedPhotos.forEach((p, i) => p.isMain = (i === index));
  }

  removePhoto(index: number, event: Event) {
    event.stopPropagation();
    const removedWasMain = this.selectedPhotos[index].isMain;
    this.selectedPhotos.splice(index, 1);
    if (removedWasMain && this.selectedPhotos.length > 0) {
      this.selectedPhotos[0].isMain = true;
    }
  }

  isControlInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!(control && control.touched && control.invalid);
  }

  cancel() { window.history.back(); }
}