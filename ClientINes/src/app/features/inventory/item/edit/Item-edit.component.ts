import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ItemService } from '../../../../shared/services/item.service';
import { LocationService } from '../../../../shared/services/location.service';
import { CategoryService } from '../../../../shared/services/category.service';
import { TranslateModule } from '@ngx-translate/core';
import { Item } from '../../../../models/entities/item.entity';
import { ITEM_STATUS_OPTIONS } from '../../../../models/constants/item-status.constants';
import { StatusNamePipe } from '../../../../shared/pipe/status-name.pipe';
import { LendingService } from '../../../../shared/services/lending.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-item-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, RouterModule, StatusNamePipe],
  templateUrl: './item-edit.component.html',
  styleUrls: ['./item-edit.component.scss']
})
export class ItemEditComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  
  private itemService = inject(ItemService);
  private locationService = inject(LocationService);
  private categoryService = inject(CategoryService);
  private lendingService = inject(LendingService);
  private authService = inject(AuthService);

  itemId!: string;
  item?: Item;
  locations: any[] = [];
  categories: any[] = [];
  selectedPhotos: { file: File, preview: string }[] = [];
  showCategoryModal = false;
  
  readonly MAX_PHOTOS = 5;
  todayMax = new Date().toISOString().split('T')[0];

  readonly statusOptions = ITEM_STATUS_OPTIONS;

  form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    description: [''],
    categoryId: [null as string | null, Validators.required],
    storageLocationId: [null as string | null, Validators.required],
    status: [{ value: 0, disabled: true }],
    purchaseDate: ['', [this.dateNotInFutureValidator]],
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
    this.itemId = this.route.snapshot.paramMap.get('id')!;
    this.loadInitialData();
    this.loadItem();

    this.form.get('status')?.valueChanges.subscribe(statusId => {
      const s = Number(statusId);
      const isLending = (s === 1 || s === 7);
      const emailControl = this.form.get('contactEmail');

      if (s === 7) {
        this.authService.user$.subscribe(user => {
          if (user?.email && !emailControl?.value) {
            emailControl?.setValue(user.email);
            emailControl?.disable();
          }
        });
      } else {
        emailControl?.enable();
      }
      this.updateLendingValidators(isLending);
    });
  }

  private loadInitialData() {
    this.locationService.getAll().subscribe(res => this.locations = res);
    this.categoryService.getAll().subscribe(res => {
      this.categories = res.sort((a: any, b: any) => a.name.localeCompare(b.name));
    });
  }

  private loadItem() {
    this.itemService.getById(this.itemId).subscribe((item: Item) => {
      this.item = item;
      
      this.form.patchValue({
        name: item.name,
        description: item.description,
        categoryId: item.categoryId,
        storageLocationId: item.storageLocationId,
        status: item.status,
        purchaseDate: item.purchaseDate ? item.purchaseDate.split('T')[0] : '',
        purchasePrice: item.purchasePrice,
        estimatedValue: item.estimatedValue
      });

      if (item.lending) {
        this.form.patchValue({
          personName: item.lending.personName,
          contactEmail: item.lending.contactEmail,
          expectedReturnDate: item.lending.expectedReturnDate ? item.lending.expectedReturnDate.split('T')[0] : null,
          sendNotification: item.lending.sendNotification
        });

        if (item.status === 7) {
          this.form.get('contactEmail')?.disable();
        }
      }

      if (item.status !== 0) {
        this.form.disable();
        this.form.get('addPhoto')?.enable();
      }
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
      }
      control?.updateValueAndValidity();
    });
  }

  returnItem() {
    this.lendingService.returnItem(this.itemId, { returnedDate: new Date().toISOString() }).subscribe({
      next: () => {
        this.loadItem();
      }
    });
  }

  private dateNotInFutureValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    const today = new Date().toISOString().split('T')[0];
    return control.value > today ? { futureDate: true } : null;
  }

  // --- ЛОГИКА ОДАЛЖИВАНИЯ ---
  get activeLending() {
    return this.item?.lending && !this.item.lending.returnedDate ? this.item.lending : null;
  }

  get activeReminders() {
    return this.item?.reminders?.filter(r => !r.isCompleted) || [];
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

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const files = Array.from(input.files);
    const availableSlots = this.MAX_PHOTOS - this.selectedPhotos.length;

    files.slice(0, availableSlots).forEach(file => {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.selectedPhotos.push({ file, preview: e.target.result });
      };
      reader.readAsDataURL(file);
    });
    input.value = ''; 
  }

  removePhoto(index: number) {
    this.selectedPhotos.splice(index, 1);
  }

  onSubmit() {
    if (this.item && this.item.status !== 0 && !this.form.get('addPhoto')?.value) {
      return; 
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const val = this.form.getRawValue();
    const formData = new FormData();

    formData.append('name', val.name!);
    formData.append('description', val.description || '');
    if (val.categoryId) formData.append('categoryId', val.categoryId);
    if (val.storageLocationId) formData.append('storageLocationId', val.storageLocationId);
    formData.append('status', val.status!.toString());

    if (val.purchaseDate) formData.append('purchaseDate', new Date(val.purchaseDate).toISOString());
    if (val.purchasePrice != null) formData.append('purchasePrice', val.purchasePrice.toString());
    if (val.estimatedValue != null) formData.append('estimatedValue', val.estimatedValue.toString());
    if (this.isLendingStatus) {
      formData.append('personName', val.personName || '');
      formData.append('contactEmail', val.contactEmail || '');
      if (val.expectedReturnDate) {
        formData.append('expectedReturnDate', new Date(val.expectedReturnDate).toISOString());
      }
      formData.append('sendNotification', (!!val.sendNotification).toString());
    }

    if (val.addPhoto && this.selectedPhotos.length > 0) {
      this.selectedPhotos.forEach(p => formData.append('photos', p.file));
    }

    this.itemService.update(this.itemId, formData).subscribe({
      next: () => this.router.navigate(['/item', this.itemId]),
      error: (err) => console.error('Update failed', err)
    });
  }

  cancel() { window.history.back(); }
}