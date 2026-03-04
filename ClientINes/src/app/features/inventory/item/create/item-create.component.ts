import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ItemService } from '../../../../shared/services/item.service';
import { LocationService } from '../../../../shared/services/location.service';
import { TranslateModule } from '@ngx-translate/core';
import { ITEM_STATUS_LABELS } from '../../../../models/enums/status-mappings';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { CategoryService } from '../../../../shared/services/category.service';

@Component({
  selector: 'app-item-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, RouterModule],
  templateUrl: './item-create.component.html',
  styleUrl: './item-create.component.css'
})
export class ItemCreateComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  
  // Сервисы из Shared
  private itemService = inject(ItemService);
  private locationService = inject(LocationService);
  private categoryService = inject(CategoryService);

  locations: any[] = [];
  categories: any[] = [];
  selectedPhotos: { file: File, preview: string }[] = [];
  
  readonly MAX_PHOTOS = 5;
  todayMax = new Date().toISOString().split('T')[0];
  
  statuses = Object.entries(ITEM_STATUS_LABELS).map(([value, label]) => ({
    value: Number(value),
    label: label
  }));

  private dateNotInFuture = (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    
    const selectedDate = control.value;
    return selectedDate > this.todayMax ? { futureDate: true } : null;
  }

  form = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    categoryId: ['', Validators.required],
    storageLocationId: ['', Validators.required],
    status: [0, Validators.required],
    purchaseDate: [this.todayMax, [this.dateNotInFuture]],
    purchasePrice: [null as number | null],
    estimatedValue: [null as number | null],
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
      if (!val) this.selectedPhotos = [];
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

  // Логика создания категории теперь через сервис
  addCategory() {
    const name = prompt('Введите название новой категории:');
    if (name && name.trim()) {
      this.categoryService.create({ name: name.trim() }).subscribe({
        next: (newCat) => {
          this.categories.push(newCat);
          this.categories.sort((a, b) => a.name.localeCompare(b.name));
          this.form.patchValue({ categoryId: newCat.id });
        },
        error: (err) => alert('Не удалось создать категорию')
      });
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
    if (val.purchaseDate) formData.append('purchaseDate', new Date(val.purchaseDate).toISOString());
    if (val.purchasePrice != null) formData.append('purchasePrice', String(val.purchasePrice));
    if (val.estimatedValue != null) formData.append('estimatedValue', String(val.estimatedValue));

    if (val.addPhoto && this.selectedPhotos.length > 0) {
      this.selectedPhotos.forEach(photo => {
        formData.append('photos', photo.file);
      });
    }

    this.itemService.createWithPhoto(formData).subscribe({
      next: () => {
        const path = val.storageLocationId ? ['/location', val.storageLocationId] : ['/home'];
        this.router.navigate(path);
      },
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
        this.selectedPhotos.push({ file: file, preview: e.target.result });
      };
      reader.readAsDataURL(file);
    });
    input.value = '';
  }

  removePhoto(index: number) {
    this.selectedPhotos.splice(index, 1);
  }

  cancel() {
    window.history.back(); 
  }
}