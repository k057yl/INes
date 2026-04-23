import { Component, Output, EventEmitter, OnInit, inject, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';
import { CategoryService } from '../../../../shared/services/category.service';

@Component({
  selector: 'app-item-filter-bar',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './item-filter-bar.component.html',
  styleUrl: './item-filter-bar.component.scss'
})
export class ItemFilterBarComponent implements OnInit {
  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private eRef = inject(ElementRef);

  @Output() filterChanged = new EventEmitter<any>();
  @Output() bulkModeToggled = new EventEmitter<boolean>();

  showFilters = false;
  showSort = false;
  isBulkMode = false;
  categories$ = this.categoryService.getAll();

  filterForm = this.fb.group({
    searchQuery: [''],
    categoryId: [null],
    status: [null as number | null],
    sortBy: [0],
    minPrice: [null],
    maxPrice: [null]
  });

  quickStatuses = [
    { value: null, label: 'FILTERS.SHOW_ALL' },
    { value: 0, label: 'STATUS.ACTIVE' },
    { value: 1, label: 'STATUS.LENT' },
    { value: 6, label: 'STATUS.LISTED' },
    { value: 4, label: 'STATUS.SOLD' }
  ];

  @HostListener('document:click', ['$event'])
  clickout(event: any) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.showFilters = false;
      this.showSort = false;
    }
  }

  ngOnInit() {
    this.filterForm.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr))
    ).subscribe(values => {
      this.filterChanged.emit(values);
    });
  }

  toggleFilters() { this.showFilters = !this.showFilters; this.showSort = false; }
  toggleSort() { this.showSort = !this.showSort; this.showFilters = false; }
  
  toggleBulkMode() {
    this.isBulkMode = !this.isBulkMode;
    this.bulkModeToggled.emit(this.isBulkMode);
  }

  setStatus(status: number | null) {
    this.filterForm.patchValue({ status });
  }

  setSort(sortBy: number) {
    this.filterForm.patchValue({ sortBy });
    this.showSort = false;
  }

  reset() {
    this.filterForm.reset({
      searchQuery: '', categoryId: null, status: null, sortBy: 0, minPrice: null, maxPrice: null
    });
  }
}