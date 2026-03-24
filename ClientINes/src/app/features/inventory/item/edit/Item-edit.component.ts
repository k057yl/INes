import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ItemService } from '../../../../shared/services/item.service';
import { LocationService } from '../../../../shared/services/location.service';
import { CategoryService } from '../../../../shared/services/category.service';
import { TranslateModule } from '@ngx-translate/core';
import { InestModalComponent } from '../../../../shared/components/modal/shared-modal/inest-modal.component';
import { Item } from '../../../../models/entities/item.entity';
import { ITEM_STATUS_OPTIONS } from '../../../../models/constants/item-status.constants';
import { StatusNamePipe } from '../../../../shared/components/pipe/status-name.pipe';
import { ReminderNamePipe } from '../../../../shared/components/pipe/reminder-name.pipe';

@Component({
  selector: 'app-item-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, RouterModule, InestModalComponent, StatusNamePipe, ReminderNamePipe],
  templateUrl: './item-edit.component.html',
  styleUrl: '../create/item-create.component.scss'
})
export class ItemEditComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  
  private itemService = inject(ItemService);
  private locationService = inject(LocationService);
  private categoryService = inject(CategoryService);

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
    status: [{ value: 0, disabled: true }, Validators.required],
    purchaseDate: ['', [this.dateNotInFutureValidator]],
    purchasePrice: [null as number | null, [Validators.min(0)]],
    estimatedValue: [null as number | null, [Validators.min(0)]],
    addPhoto: [false]
  });

  ngOnInit() {
    this.itemId = this.route.snapshot.paramMap.get('id')!;
    this.loadInitialData();
    this.loadItem();
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

      if (item.status !== 0) {
        this.form.disable();
      } else {
        this.form.get('status')?.disable();
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
    if (this.item && this.item.status !== 0) {
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
    if (val.addPhoto && this.selectedPhotos.length > 0) {
      this.selectedPhotos.forEach(p => formData.append('photos', p.file));
    }

    this.itemService.update(this.itemId, formData).subscribe({
      next: () => this.router.navigate(['/item', this.itemId]),
      error: (err) => console.error('Update failed', err)
    });
  }

  returnItem() {
    console.log('Логика возврата предмета будет здесь');
    // В будущем тут будет вызов: this.itemService.returnItem(this.itemId).subscribe(...)
  }

  cancel() { window.history.back(); }
}