import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ItemService } from '../../../services/item.service';
import { LocationService } from '../../../services/location.service';
import { CategoryService } from '../../../services/category.service';
import { TranslateModule } from '@ngx-translate/core';
import { ITEM_STATUS_LABELS } from '../../../models/enums/status-mappings';

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
    storageLocationId: ['', Validators.required],
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
    });
  }

  cancel() {
    window.history.back(); 
  }
}