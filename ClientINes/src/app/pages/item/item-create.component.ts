import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ItemService } from '../../services/item.service';
import { LocationService } from '../../services/location.service';
import { CategoryService } from '../../services/category.service';
import { TranslateModule } from '@ngx-translate/core';
import { ITEM_STATUS_LABELS } from '../../models/enums/status-mappings';

@Component({
  selector: 'app-item-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  template: `
    <div class="container">
      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="item-form">
        <h2>{{ 'ITEM.CREATE_TITLE' | translate }}</h2>

        <div class="input-group">
          <label>{{ 'ITEM.NAME' | translate }}</label>
          <input formControlName="name" [placeholder]="'ITEM.NAME_PLACE' | translate" required>
        </div>

        <div class="input-group">
          <label>{{ 'ITEM.DESCRIPTION' | translate }}</label>
          <textarea formControlName="description" [placeholder]="'ITEM.DESC_PLACE' | translate"></textarea>
        </div>

        <div class="row">
          <div class="input-group">
            <label>{{ 'ITEM.CATEGORY' | translate }}</label>
            <select formControlName="categoryId" required>
              <option value="">{{ 'ITEM.SELECT_CAT' | translate }}</option>
              <option *ngFor="let cat of categories" [value]="cat.id">{{ cat.name }}</option>
            </select>
          </div>

          <div class="input-group">
            <label>{{ 'ITEM.STATUS' | translate }}</label>
            <select formControlName="status">
              <option *ngFor="let s of statuses" [value]="s.value">
                {{ s.label | translate }} 
              </option>
            </select>
          </div>
        </div>

        <div class="input-group">
          <label>{{ 'ITEM.LOCATION' | translate }}</label>
          <select formControlName="storageLocationId">
            <option value="">{{ 'ITEM.NO_LOCATION' | translate }}</option>
            <option *ngFor="let loc of locations" [value]="loc.id">{{ loc.name }}</option>
          </select>
        </div>

        <div class="input-group">
          <label>{{ 'ITEM.PURCHASE_DATE' | translate }}</label>
          <input type="date" formControlName="purchaseDate" [max]="todayMax">
          <small class="error-text" *ngIf="form.get('purchaseDate')?.errors?.['futureDate']">
            {{ 'ERROR.FUTURE_DATE' | translate }}
          </small>
        </div>

        <div class="row">
          <div class="input-group">
            <label>{{ 'ITEM.PRICE' | translate }}</label>
            <input formControlName="purchasePrice" type="number" [placeholder]="'ITEM.PRICE_PLACE' | translate">
          </div>
          <div class="input-group">
            <label>{{ 'ITEM.ESTIMATED' | translate }}</label>
            <input formControlName="estimatedValue" type="number" [placeholder]="'ITEM.EST_PLACE' | translate">
          </div>
        </div>

        <label class="photo-flag">
          <input type="checkbox" formControlName="addPhoto">
          <span>{{ 'ITEM.ADD_PHOTO' | translate }}</span>
        </label>

        <div *ngIf="form.value.addPhoto" class="file-upload-zone">
          <input type="file" accept="image/*" (change)="onFileSelected($event)">
        </div>

        <div class="actions">
          <button type="button" class="btn-cancel" (click)="cancel()">
            {{ 'COMMON.CANCEL' | translate }}
          </button>
          <button type="submit" class="btn-save" [disabled]="form.invalid">
            {{ 'COMMON.SAVE' | translate }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .container { padding: 40px 20px; display: flex; justify-content: center; min-height: 90vh; }
    
    .item-form {
      background: #1c2541;
      padding: 30px;
      border-radius: 16px;
      width: 100%;
      max-width: 600px;
      color: white;
      box-shadow: 0 15px 35px rgba(0,0,0,0.4);
      border: 1px solid #3a506b;
    }

    h2 { color: #00f5d4; margin-bottom: 25px; text-align: center; font-weight: 800; }
    
    .input-group { margin-bottom: 15px; flex: 1; }
    
    label { display: block; margin-bottom: 6px; font-size: 0.85rem; color: #94a3b8; }

    input, select, textarea {
      width: 100%;
      padding: 12px;
      background: #0b132b;
      border: 1px solid #3a506b;
      border-radius: 8px;
      color: white;
      font-size: 0.95rem;
      transition: all 0.3s;
    }

    input:focus, select:focus, textarea:focus {
      border-color: #00f5d4;
      outline: none;
      box-shadow: 0 0 10px rgba(0, 245, 212, 0.2);
    }

    .row { display: flex; gap: 15px; }

    .photo-flag {
      display: flex;
      align-items: center;
      gap: 10px;
      margin: 20px 0;
      cursor: pointer;
      color: #00f5d4;
      font-weight: 600;
    }

    .error-text { color: #ff4d4d; font-size: 0.75rem; margin-top: 4px; display: block; }

    .actions { display: flex; gap: 15px; margin-top: 30px; }

    button {
      flex: 1;
      padding: 14px;
      border-radius: 8px;
      border: none;
      font-weight: 700;
      cursor: pointer;
      transition: 0.3s;
    }

    .btn-cancel { background: #3a506b; color: white; }
    .btn-save { background: #00f5d4; color: #0b132b; }

    .btn-save:hover:not(:disabled) {
      box-shadow: 0 0 20px rgba(0, 245, 212, 0.4);
      transform: translateY(-2px);
    }

    button:disabled { opacity: 0.3; cursor: not-allowed; }

    .file-upload-zone {
      padding: 15px;
      border: 2px dashed #3a506b;
      border-radius: 8px;
      text-align: center;
    }
  `]
})
export class ItemCreateComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);
  private itemService = inject(ItemService);
  private locationService = inject(LocationService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  locations: any[] = [];
  categories: any[] = [];
  selectedFile?: File;

  todayMax = new Date().toISOString().split('T')[0];

  statuses = Object.entries(ITEM_STATUS_LABELS).map(([value, label]) => ({
    value: Number(value),
    label: label
  }));

  form = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    categoryId: ['', Validators.required],
    storageLocationId: [''],
    status: [0, Validators.required],
    purchaseDate: [this.todayMax, [this.dateNotInFuture]],
    purchasePrice: [null as number | null],
    estimatedValue: [null as number | null],
    addPhoto: [false]
  });

  private dateNotInFuture(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  
  const selectedDate = control.value;
  const today = new Date().toISOString().split('T')[0];

  return selectedDate > today ? { futureDate: true } : null;
}

  ngOnInit() {
    this.loadLocations();
    this.loadCategories();

    this.route.queryParams.subscribe(params => {
      const locationIdFromUrl = params['locationId'];
      if (locationIdFromUrl) {
        this.form.patchValue({ storageLocationId: locationIdFromUrl });
      }
    });

    this.form.get('addPhoto')?.valueChanges.subscribe(val => {
      if (!val) this.selectedFile = undefined;
    });
  }

  loadLocations() {
    this.locationService.getAll().subscribe({
      next: res => this.locations = res,
      error: err => console.error('Ошибка загрузки локаций', err)
    });
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: res => this.categories = res,
      error: err => console.error('Ошибка загрузки категорий', err)
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

  onSubmit() {
    if (this.form.invalid) return;

    const val = this.form.value;
    const formData = new FormData();

    formData.append('name', val.name || '');
    formData.append('description', val.description || '');
    formData.append('categoryId', val.categoryId || '');
    formData.append('status', String(val.status ?? 0));

    if (val.storageLocationId) formData.append('storageLocationId', val.storageLocationId);
    
    if (val.purchaseDate) {
        formData.append('purchaseDate', new Date(val.purchaseDate).toISOString());
    }

    if (val.purchasePrice != null) formData.append('purchasePrice', String(val.purchasePrice));
    if (val.estimatedValue != null) formData.append('estimatedValue', String(val.estimatedValue));
    if (val.addPhoto && this.selectedFile) formData.append('photo', this.selectedFile);

    this.itemService.createWithPhoto(formData).subscribe({
      next: () => {
        if (val.storageLocationId) {
          this.router.navigate(['/location', val.storageLocationId]);
        } else {
          this.router.navigate(['/home']);
        }
      },
      error: err => console.error('Ошибка при создании:', err)
    });
  }

  cancel() {
    window.history.back(); 
  }
}