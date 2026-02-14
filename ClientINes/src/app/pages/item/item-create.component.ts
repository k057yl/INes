import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ItemService } from '../../services/item.service';
import { LocationService } from '../../services/location.service';
import { CategoryService } from '../../services/category.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-item-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container">
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <h2>Создать айтем</h2>

        <input formControlName="name" placeholder="Название" required>

        <textarea formControlName="description" placeholder="Описание"></textarea>

        <select formControlName="categoryId" required>
          <option value="">Выберите категорию</option>
          <option *ngFor="let cat of categories" [value]="cat.id">
            {{ cat.name }}
          </option>
        </select>

        <select formControlName="storageLocationId">
          <option value="">Без локации</option>
          <option *ngFor="let loc of locations" [value]="loc.id">
            {{ loc.name }}
          </option>
        </select>

        <select formControlName="status">
          <option *ngFor="let s of statuses" [value]="s.value">
            {{ s.label }}
          </option>
        </select>

        <input formControlName="purchaseDate" type="date">

        <input formControlName="purchasePrice" type="number" placeholder="Цена покупки">

        <input formControlName="estimatedValue" type="number" placeholder="Оценочная стоимость">

        <label class="photo-flag">
          <input type="checkbox" formControlName="addPhoto">
          Добавить фото
        </label>

        <div *ngIf="form.value.addPhoto">
          <input type="file" accept="image/*" (change)="onFileSelected($event)">
        </div>

        <div class="actions">
          <button type="button" (click)="cancel()">Отмена</button>
          <button type="submit" [disabled]="form.invalid">Сохранить</button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
      max-width: 500px;
      margin: auto;
    }

    input, select, textarea {
      display: block;
      width: 100%;
      margin-bottom: 10px;
      padding: 8px;
    }

    .actions {
      display: flex;
      gap: 10px;
    }

    .photo-flag {
      display: flex;
      align-items: center;
      gap: 6px;
      margin-bottom: 10px;
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

  statuses = [
    { value: 0, label: 'Active' },
    { value: 1, label: 'Lent' },
    { value: 2, label: 'Lost' },
    { value: 3, label: 'Broken' },
    { value: 4, label: 'Sold' },
    { value: 5, label: 'Gifted' }
  ];

  form = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    categoryId: ['', Validators.required],
    storageLocationId: [''],
    status: [0, Validators.required],
    purchaseDate: [''],
    purchasePrice: [null],
    estimatedValue: [null],
    addPhoto: [false]
  });

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

    if (!this.form.valid) return;

    const val = this.form.value;

    const formData = new FormData();

    formData.append('name', val.name || '');
    formData.append('description', val.description || '');
    formData.append('categoryId', val.categoryId || '');
    formData.append('status', String(val.status ?? 0));

    if (val.storageLocationId)
      formData.append('storageLocationId', val.storageLocationId);

    if (val.purchaseDate)
      formData.append('purchaseDate', new Date(val.purchaseDate).toISOString());

    if (val.purchasePrice != null)
      formData.append('purchasePrice', String(val.purchasePrice));

    if (val.estimatedValue != null)
      formData.append('estimatedValue', String(val.estimatedValue));

    if (val.addPhoto && this.selectedFile)
      formData.append('photo', this.selectedFile);

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