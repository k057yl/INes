import { Component, inject, OnInit, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs';

import { ItemService } from '../../../../shared/services/item.service';
import { CategoryService } from '../../../../shared/services/category.service';
import { LocationService } from '../../../../shared/services/location.service';
import { Item } from '../../../../models/entities/item.entity';
import { StatusNamePipe } from '../../../../shared/pipe/status-name.pipe';
import { DashboardModalService } from '../../../dashboard/dashboard.modal.service';

@Component({
  selector: 'app-items-list',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, ReactiveFormsModule, StatusNamePipe],
  templateUrl: './items-list.component.html',
  styleUrl: './items-list.component.scss'
})
export class ItemsListComponent implements OnInit {
  private itemService = inject(ItemService);
  private categoryService = inject(CategoryService);
  private locationService = inject(LocationService);
  private modalService = inject(DashboardModalService);
  private fb = inject(FormBuilder);
  private eRef = inject(ElementRef);

  items: Item[] = [];
  categories: any[] = [];
  locations: any[] = [];
  isLoading = true;
  
  selectedIds = new Set<string>();
  
  activeDropdown: 'category' | 'location' | 'status' | 'sort' | null = null;

  filterForm = this.fb.group({
    searchQuery: [''],
    categoryId: [null as string | null],
    storageLocationId: [null as string | null],
    status: [null as number | null],
    sortBy: [0]
  });

  readonly STATUSES = [
    { value: null, label: 'FILTERS.SHOW_ALL' },
    { value: 0, label: 'STATUS.ACTIVE' },
    { value: 1, label: 'STATUS.LENT' },
    { value: 6, label: 'STATUS.LISTED' },
    { value: 4, label: 'STATUS.SOLD' }
  ];

  trackById = (index: number, item: any) => item.id;

  @HostListener('document:click', ['$event'])
  clickout(event: any) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.activeDropdown = null;
    }
  }

  ngOnInit() {
    this.categoryService.getAll().subscribe(res => this.categories = res);
    this.locationService.getAll().subscribe(res => this.locations = res);

    this.filterForm.get('searchQuery')?.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(() => this.loadData());

    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    const filters = this.filterForm.getRawValue();
    this.itemService.getAll(filters)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(data => {
        this.items = data;
        this.selectedIds.clear(); 
      });
  }

  toggleSort(asc: number, desc: number) {
    const currentSort = this.filterForm.get('sortBy')?.value;
    const nextSort = currentSort === asc ? desc : asc;
    
    this.filterForm.patchValue({ sortBy: nextSort }, { emitEvent: false });
    this.activeDropdown = null;
    this.loadData();
  }

  toggleDropdown(menu: 'category' | 'location' | 'status' | 'sort', event: Event) {
    event.stopPropagation();
    this.activeDropdown = this.activeDropdown === menu ? null : menu;
  }

  setFilter(field: 'categoryId' | 'storageLocationId' | 'status' | 'sortBy' | 'searchQuery', value: any) {
    this.filterForm.patchValue({ [field]: value }, { emitEvent: false });
    this.activeDropdown = null;
    this.loadData();
  }

  getSortIcon(asc: number, desc: number): string {
    const s = this.filterForm.get('sortBy')?.value;
    if (s === asc) return 'fa-sort-amount-up active-sort';
    if (s === desc) return 'fa-sort-amount-down active-sort';
    return 'fa-sort muted-sort';
  }

  toggleSelection(id: string) {
    if (this.selectedIds.has(id)) this.selectedIds.delete(id);
    else this.selectedIds.add(id);
  }

  toggleAll(event: Event) {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.items.forEach(i => this.selectedIds.add(i.id));
    } else {
      this.selectedIds.clear();
    }
  }

  isAllSelected(): boolean {
    return this.items.length > 0 && this.selectedIds.size === this.items.length;
  }

  bulkDelete() {
    if (this.selectedIds.size === 0) return;

    this.modalService.openConfirm({
      mode: 'delete',
      title: 'COMMON.DELETE',
      message: `Удалить выбранные предметы (${this.selectedIds.size} шт.)?`
    }).subscribe(res => {
      if (res) {
        this.isLoading = true;
        this.itemService.deleteBatch(Array.from(this.selectedIds)).subscribe({
          next: () => this.loadData(),
          error: () => this.isLoading = false
        });
      }
    });
  }

  onEditClick(item: Item) {
    this.modalService.openItemForm(item).subscribe(res => { if (res) this.loadData(); });
  }

  onDeleteClick(item: Item) {
    this.selectedIds.clear();
    this.selectedIds.add(item.id);
    this.bulkDelete();
  }
}