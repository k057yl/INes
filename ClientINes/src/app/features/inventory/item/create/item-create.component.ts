import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ItemService } from '../../../../shared/services/item.service';
import { LocationService } from '../../../../shared/services/location.service';
import { CategoryService } from '../../../../shared/services/category.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { InestModalComponent } from '../../../../shared/components/modal/shared-modal/inest-modal.component';
import { ITEM_STATUS_OPTIONS } from '../../../../models/constants/item-status.constants';

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
    categoryId: ['', Validators.required],
    storageLocationId: ['', Validators.required],
    status: [0, Validators.required],
    purchaseDate: [this.todayMax, [this.dateNotInFutureValidator]],
    purchasePrice: [null as number | null, [Validators.min(0)]],
    estimatedValue: [null as number | null, [Validators.min(0)]],
    addPhoto: [false]
  });

  ngOnInit() {
    this.loadData();

    this.route.queryParams.subscribe(params => {
      if (params['locationId']) {
        this.form.patchValue({ storageLocationId: params['locationId'] });
      }
    });

    this.form.get('addPhoto')?.valueChanges.subscribe(val => {
      if (!val) this.selectedPhotos = [];
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

    if (val.purchaseDate) {
      formData.append('purchaseDate', new Date(val.purchaseDate).toISOString());
    }

    if (val.purchasePrice != null) {
      formData.append('purchasePrice', val.purchasePrice.toString());
      formData.append('estimatedValue', (val.estimatedValue ?? val.purchasePrice).toString());
    }

    if (val.addPhoto) {
      this.selectedPhotos.forEach(p => formData.append('photos', p.file));
    }

    this.itemService.createWithPhoto(formData).subscribe({
      next: () => {
        this.router.navigate(['/location', val.storageLocationId]);
      }
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

  cancel() { window.history.back(); }
}