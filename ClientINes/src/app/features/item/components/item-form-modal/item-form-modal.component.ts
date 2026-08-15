import { Component, inject, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';

import { ItemService } from '../../services/item.service';
import { LocationService } from '../../../location/services/location.service';
import { CategoryService } from '../../../category/services/category.service';
import { LendingService } from '../../../lending/services/lending.service';
import { AuthService } from '../../../auth/services/auth.service';
import { LocalizationService } from '../../../../core/services/localization.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { StatusNamePipe } from '../../../../shared/pipes/status-name.pipe';
import { InestModalComponent } from '../../../../shared/components/inest-modal/inest-modal.component';

import { ITEM_STATUS_OPTIONS } from '../../../../core/constants/item-status.constants';
import { Item } from '../../contracts/item';
import { ReminderType } from '../../../reminder/enums/reminder-type.enum';
import { ReminderRecurrence } from '../../../reminder/enums/reminder-recurrence.enum';

import { take, filter } from 'rxjs/operators';
import { FormErrorService } from '../../../../core/services/form-error.service';
import { ToastrService } from 'ngx-toastr';
import { TutorialService, TutorialStep } from '../../../../core/services/tutorial.service';

interface PhotoSlot {
  file?: File;
  preview: string;
  isMain: boolean;
}

@Component({
  selector: 'app-item-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, RouterModule, StatusNamePipe, InestModalComponent],
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
  private tutorialService = inject(TutorialService);

  locations: any[] = [];
  categories: any[] = [];
  selectedPhotos: PhotoSlot[] = [];
  isLocationPredefined = false;
  showCategoryModal = false;

  selectedReceiptFile: File | null = null;
  selectedReceiptFileName: string | null = null;
  
  readonly MAX_PHOTOS = 5;
  todayMax = new Date().toISOString().split('T')[0];
  readonly statusOptions = ITEM_STATUS_OPTIONS;

  readonly reminderTypeOptions = [
    { value: ReminderType.Custom, label: 'REMINDERS.CUSTOM' },
    { value: ReminderType.Warranty, label: 'REMINDERS.WARRANTY' },
    { value: ReminderType.Maintenance, label: 'REMINDERS.MAINTENANCE' },
    { value: ReminderType.Insurance, label: 'REMINDERS.INSURANCE' },
    { value: ReminderType.Medical, label: 'REMINDERS.MEDICAL' },
    { value: ReminderType.Tax, label: 'REMINDERS.TAX' },
    { value: ReminderType.Subscription, label: 'REMINDERS.SUBSCRIPTION' }
  ];

  readonly reminderRecurrenceOptions = [
    { value: ReminderRecurrence.None, label: 'RECURRENCE.NONE' },
    { value: ReminderRecurrence.Daily, label: 'RECURRENCE.DAILY' },
    { value: ReminderRecurrence.Weekly, label: 'RECURRENCE.WEEKLY' },
    { value: ReminderRecurrence.Monthly, label: 'RECURRENCE.MONTHLY' },
    { value: ReminderRecurrence.Yearly, label: 'RECURRENCE.YEARLY' }
  ];

  form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    description: [''],
    categoryId: ['', Validators.required],
    storageLocationId: ['', Validators.required],
    status: [0, Validators.required],
    
    addDetails: [false],
    currency: ['USD'],
    purchaseDate: [this.todayMax, [this.dateNotInFutureValidator]],
    purchasePrice: [null as number | null, [Validators.min(0)]],

    personName: [''],
    contactEmail: ['', [Validators.email]],
    expectedReturnDate: [null as string | null],
    sendNotification: [false],
    sendTelegramNotification: [true],
    addPhoto: [false],

    addReceipt: [false],
    warrantyExpiration: [null as string | null],

    addReminder: [false],
    reminderTitle: [''],
    reminderType: [ReminderType.Custom],
    reminderRecurrence: [ReminderRecurrence.None],
    reminderTriggerAt: [null as string | null],
    reminderSendNotification: [false],
    reminderSendTelegramNotification: [true]
  });

  get isEdit(): boolean { return !!this.item; }
  get isLendingStatus(): boolean {
    const s = Number(this.form.getRawValue().status);
    return s === 1 || s === 4;
  }
  get currentStatusNumber(): number {
    return Number(this.form.getRawValue().status);
  }

  get availableStatuses() {
    if (this.isEdit) {
      return this.statusOptions.filter(opt => opt.value === this.item?.status);
    }
    return this.statusOptions.filter(opt => [0, 1, 4].includes(opt.value));
  }

  ngOnInit() {
    this.loadInitialData();

    this.form.get('status')?.valueChanges.subscribe(val => this.applyLendingLogic(val));
    this.form.get('addReminder')?.valueChanges.subscribe(val => this.updateReminderValidators(!!val));
    this.form.get('addReceipt')?.valueChanges.subscribe(val => {
      if (!val) {
        this.form.patchValue({ warrantyExpiration: null });
        this.selectedReceiptFile = null;
        this.selectedReceiptFileName = null;
      }
    });

    if (!this.isEdit) {
      this.checkAndStartTutorial();
    }

    if (this.isEdit && this.item) {
      this.patchFormValues(this.item);
    } else {
      this.setupNewItemDefaults();
    }

    this.applyLendingLogic(this.form.get('status')?.value);
  }

  private checkAndStartTutorial() {
    this.authService.user$.pipe(take(1)).subscribe(user => {
      if (!user) return;

      const isFormPassed = (user.completedTutorials & TutorialStep.ItemForm) === TutorialStep.ItemForm;

      if (!isFormPassed) {
        setTimeout(() => {
          this.tutorialService.startItemFormTour(() => {
            user.completedTutorials |= TutorialStep.ItemForm;
            this.authService.updateLocalUserTutorial(TutorialStep.ItemForm);
          });
        }, 400);
      }
    });
  }

  openCategoryModal() { this.showCategoryModal = true; }
  closeCategoryModal() { this.showCategoryModal = false; }

  saveNewCategory(name: string) {
    this.categoryService.create({ name }).subscribe({
      next: (res: any) => {
        this.closeCategoryModal();
        this.categoryService.getAll().subscribe(cats => {
          this.categories = cats.sort((a: any, b: any) => a.name.localeCompare(b.name));
          const newCatId = res?.id || cats.find((c: any) => c.name === name)?.id;
          if (newCatId) this.form.get('categoryId')?.setValue(newCatId);
        });
      },
      error: () => {
        this.toastr.error(this.translateService.instant('SYSTEM.DEFAULT_ERROR'));
        this.closeCategoryModal();
      }
    });
  }

  private applyLendingLogic(statusId: any) {
    const s = Number(statusId);
    const emailControl = this.form.get('contactEmail');
    const isLending = (s === 1 || s === 4);

    if (isLending) {
      this.form.patchValue({ addReminder: false });
      
      if (s === 1) {
        this.authService.user$.pipe(
          filter(u => !!u && Object.keys(u).length > 0),
          take(1)
        ).subscribe(user => {
          const u = user as any;
          const foundEmail = u.email || u.Email || u.emailAddress || u.userName || u['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
          if (foundEmail) {
            emailControl?.setValue(foundEmail, { emitEvent: false });
            emailControl?.disable({ emitEvent: false });
          } else {
            emailControl?.enable({ emitEvent: false });
          }
        });
      }
    } else {
      emailControl?.enable({ emitEvent: false });
      if (s === 0) emailControl?.setValue('', { emitEvent: false });
    }

    this.updateLendingValidators(isLending);
  }

  private updateReminderValidators(isEnabled: boolean) {
    const dateControl = this.form.get('reminderTriggerAt');

    if (isEnabled) {
      dateControl?.setValidators([Validators.required]);
    } else {
      dateControl?.clearValidators();
    }
    dateControl?.updateValueAndValidity();
  }

  private setupNewItemDefaults() {
    this.form.patchValue({ currency: this.localizationService.getDefaultCurrency() });
    if (this.parentId) {
      this.form.patchValue({ storageLocationId: this.parentId });
      this.isLocationPredefined = true;
    }
  }

  private patchFormValues(item: Item) {
    const hasPrice = item.details?.purchasePrice !== null && item.details?.purchasePrice !== undefined;
    const hasDate = !!item.details?.purchaseDate;

    this.form.patchValue({
      name: item.name,
      description: item.description,
      categoryId: item.categoryId,
      storageLocationId: item.storageLocationId,
      status: item.status,
      currency: item.details?.currency || 'USD',
      purchaseDate: item.details?.purchaseDate ? item.details.purchaseDate.split('T')[0] : '',
      purchasePrice: item.details?.purchasePrice ?? null,
      warrantyExpiration: item.details?.warrantyExpiration ? item.details.warrantyExpiration.split('T')[0] : null
    });

    if (hasPrice || hasDate) {
      this.form.patchValue({ addDetails: true });
    }

    if (item.details?.warrantyExpiration || item.details?.receiptDocumentPath) {
      this.form.patchValue({ addReceipt: true });
    }

    if (item.lending) {
      this.form.patchValue({
        personName: item.lending.personName,
        contactEmail: item.lending.contactEmail,
        expectedReturnDate: item.lending.expectedReturnDate?.split('T')[0],
        sendNotification: item.lending.sendNotification,
        sendTelegramNotification: (item.lending as any).sendTelegramNotification
      });
    }

    if (this.isEdit) {
      this.form.get('status')?.disable({ emitEvent: false });
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

  onReceiptSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    this.selectedReceiptFile = file;
    this.selectedReceiptFileName = file.name;
  }

  removeReceipt(event: Event) {
    event.stopPropagation();
    this.selectedReceiptFile = null;
    this.selectedReceiptFileName = null;
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

    // ФИНАНСЫ: Шлём ключи без префикса "details.", ровно так, как ждёт UpdateItemFullCommand
    const hasPriceValue = val.purchasePrice !== null && val.purchasePrice !== undefined;
    if (val.addDetails || hasPriceValue || val.purchaseDate) {
      formData.append('currency', val.currency || 'USD');
      
      if (val.purchaseDate) {
        formData.append('purchaseDate', val.purchaseDate);
      }
      
      if (hasPriceValue) {
        formData.append('purchasePrice', val.purchasePrice!.toString());
      }
    }

    // ЧЕК И ГАРАНТИЯ
    if (val.addReceipt) {
      if (val.warrantyExpiration) formData.append('warrantyExpiration', val.warrantyExpiration);
      if (this.selectedReceiptFile) formData.append('receiptFile', this.selectedReceiptFile, this.selectedReceiptFile.name);
    }

    // АРЕНДА
    if (this.isLendingStatus) {
      formData.append('personName', val.personName || '');
      formData.append('contactEmail', val.contactEmail || '');
      if (val.expectedReturnDate) formData.append('expectedReturnDate', val.expectedReturnDate);
      formData.append('sendNotification', (!!val.sendNotification).toString());
      formData.append('sendTelegramNotification', (!!val.sendTelegramNotification).toString());
    }

    // НАПОМИНАНИЯ
    if (val.addReminder && val.reminderTriggerAt && !this.isLendingStatus) {
      const selectedTypeObj = this.reminderTypeOptions.find(o => o.value === val.reminderType);
      const fallbackLabel = this.translateService.instant('REMINDERS.CUSTOM');
      const typeLabel = selectedTypeObj ? this.translateService.instant(selectedTypeObj.label) : fallbackLabel;
      const autoTitle = val.name ? `${typeLabel}: ${val.name}` : typeLabel;

      formData.append('reminder.title', autoTitle);
      formData.append('reminder.type', (val.reminderType ?? ReminderType.Custom).toString());
      formData.append('reminder.recurrence', (val.reminderRecurrence ?? ReminderRecurrence.None).toString());
      formData.append('reminder.triggerAt', val.reminderTriggerAt);
      formData.append('reminder.sendNotification', (!!val.reminderSendNotification).toString());
      formData.append('reminder.sendTelegram', (!!val.reminderSendTelegramNotification).toString());
    }

    // ФОТОГРАФИИ
    if ((!this.isEdit || val.addPhoto) && this.selectedPhotos.length > 0) {
      this.selectedPhotos.forEach(p => {
        if (p.file) {
          formData.append('photos', p.file);
          if (p.isMain) formData.append('mainPhotoName', p.file.name);
        }
      });
    }

    const request$: Observable<any> = this.isEdit 
      ? this.itemService.update(this.item!.id, formData) 
      : this.itemService.createWithPhoto(formData);

    request$.subscribe({
      next: (res: any) => {
        const successKey = res?.message || (this.isEdit ? 'ITEMS.SUCCESS.UPDATE' : 'ITEMS.SUCCESS.CREATE');
        this.toastr.success(this.translateService.instant(successKey));
        this.modalService.confirm(res);
      },
      error: (err: any) => {
        if (err.details) this.formErrorService.mapServerErrorsToForm(this.form, err.details);
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
              const compressedFile = new File([blob], file.name.replace(/\.[^/.]+$/, ".jpg"), { type: 'image/jpeg', lastModified: Date.now() });
              resolve({ file: compressedFile, preview });
            } else { reject('Blob creation failed'); }
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
        this.selectedPhotos.push({ file: compressed.file, preview: compressed.preview, isMain: this.selectedPhotos.length === 0 });
      } catch (err) { console.error('Ошибка при обработке фото:', err); }
    }
    input.value = '';
  }

  isControlInvalid(name: string): boolean {
    const c = this.form.get(name);
    return !!(c && (c.touched || c.errors?.['serverError']) && c.invalid);
  }

  cancel(): void { this.modalService.close(); }
}