import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ItemService } from '../../services/item.service';
import { LocationService } from '../../services/location.service';
import { CategoryService } from '../../services/category.service';
import { ItemStatus } from '../../models/enums/item-status.enum';
import { CreateItemDto } from '../../models/dtos/item.dto';

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
          <option *ngFor="let cat of categories" [value]="cat.id">{{ cat.name }}</option>
        </select>
        
        <select formControlName="storageLocationId">
          <option value="">Без локации</option>
          <option *ngFor="let loc of locations" [value]="loc.id">{{ loc.name }}</option>
        </select>
        
        <select formControlName="status">
          <option *ngFor="let s of statuses" [value]="s.value">{{ s.label }}</option>
        </select>

        <input formControlName="purchaseDate" type="date">
        <input formControlName="purchasePrice" type="number" placeholder="Цена покупки">
        <input formControlName="estimatedValue" type="number" placeholder="Оценочная стоимость">

        <div class="actions">
          <button type="button" (click)="cancel()">Отмена</button>
          <button type="submit" [disabled]="form.invalid">Сохранить</button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .container { padding: 20px; max-width: 500px; margin: auto; }
    input, select, textarea { display: block; width: 100%; margin-bottom: 10px; padding: 8px; }
    .actions { display: flex; gap: 10px; }
  `]
})
export class ItemCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private itemService = inject(ItemService);
  private locationService = inject(LocationService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  locations: any[] = [];
  categories: any[] = [];
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
    estimatedValue: [null]
  });

  ngOnInit() {
    this.loadLocations();
    this.loadCategories();
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

  onSubmit() {
  if (this.form.valid) {
    const val = this.form.value;

    const dto: CreateItemDto = {
      name: val.name || '',
      description: val.description || '',
      categoryId: val.categoryId || '',
      storageLocationId: val.storageLocationId || undefined,
      status: val.status != null ? +val.status : 0,           // приводим к number
      purchaseDate: val.purchaseDate ? new Date(val.purchaseDate).toISOString() : undefined, // к string ISO
      purchasePrice: val.purchasePrice != null ? +val.purchasePrice : undefined,
      estimatedValue: val.estimatedValue != null ? +val.estimatedValue : undefined
    };

    this.itemService.create(dto).subscribe({
      next: () => this.router.navigate(['/home']),
      error: err => console.error(err)
    });
  }
}

  cancel() { this.router.navigate(['/home']); }
}