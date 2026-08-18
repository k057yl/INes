import { Component, HostListener, OnInit, inject, ElementRef } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, finalize, switchMap, tap, startWith, catchError, take } from 'rxjs/operators';
import { of } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { PricePipe } from '../../../../shared/pipes/price-currency.pipe';

import { ItemService } from '../../services/item.service';
import { CategoryService } from '../../../category/services/category.service';
import { LocationService } from '../../../location/services/location.service';
import { Item } from '../../contracts/item';
import { GetItemFilters } from '../../dtos/items-get.dto';
import { StatusNamePipe } from '../../../../shared/pipes/status-name.pipe';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { AuthService } from '../../../auth/services/auth.service';
import { TutorialService, TutorialStep } from '../../../../core/services/tutorial.service';

type DropdownType = 'category' | 'location' | 'status' | 'sort' | null;

@Component({
  selector: 'app-items-list',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, ReactiveFormsModule, StatusNamePipe, PricePipe],
  templateUrl: './items-list.component.html',
  styleUrl: './items-list.component.scss'
})
export class ItemsListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private itemService = inject(ItemService);
  private categoryService = inject(CategoryService);
  private locationService = inject(LocationService);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);
  private eRef = inject(ElementRef);
  private location = inject(Location);
  private router = inject(Router);

  public modal = inject(DashboardModalService);

  private authService = inject(AuthService);
  private tutorialService = inject(TutorialService);

  items: Item[] = [];
  categories: any[] = [];
  locations: any[] = [];
  isLoading = false;
  selectedIds = new Set<string>();
  activeDropdown: DropdownType = null;

  readonly STATUSES = [
    { value: null, label: 'FILTERS.SHOW_ALL' },
    { value: 0, label: 'STATUS.ACTIVE' },
    { value: 1, label: 'STATUS.LENT' },
    { value: 2, label: 'STATUS.SOLD' },
    { value: 3, label: 'STATUS.ARCHIVED' }
  ];

  filterForm = this.fb.group({
    searchQuery: [''],
    categoryId: [null as string | null],
    storageLocationId: [null as string | null],
    status: [null as number | null],
    sortBy: [0],
    minPrice: [null as number | null],
    maxPrice: [null as number | null],
    showArchived: [false],
    includeArchived: [false]
  });

  trackById = (_: number, item: Item) => item.id;

  get hasActiveFilters(): boolean {
    const f = this.filterForm.getRawValue();
    return !!(f.searchQuery || f.categoryId || f.storageLocationId || f.status !== null || f.minPrice !== null || f.maxPrice !== null);
  }

  ngOnInit(): void {
    this.loadInitialData();

    this.filterForm.get('showArchived')?.valueChanges.subscribe(val => {
      if (val) this.filterForm.get('includeArchived')?.setValue(false, { emitEvent: false });
    });

    this.filterForm.get('includeArchived')?.valueChanges.subscribe(val => {
      if (val) this.filterForm.get('showArchived')?.setValue(false, { emitEvent: false });
    });

    this.filterForm.valueChanges.pipe(
      startWith(this.filterForm.getRawValue()),
      debounceTime(300),
      tap(() => { this.isLoading = true; }),
      switchMap(filters => this.itemService.getAll(filters as GetItemFilters).pipe(
        catchError(err => {
          console.error('Ошибка бэкенда при поиске:', err);
          return of([]);
        }),
        finalize(() => { this.isLoading = false; })
      ))
    ).subscribe(items => {
      this.items = items;
      this.syncSelection();
      this.checkAndStartTutorial();
    });
  }

  private checkAndStartTutorial() {
    this.authService.user$.pipe(take(1)).subscribe(user => {
      if (!user) return;

      const isItemsListPassed = (user.completedTutorials & TutorialStep.ItemsList) === TutorialStep.ItemsList;
      if (!isItemsListPassed) {
        setTimeout(() => {
          this.tutorialService.startItemsListTour(() => {
            user.completedTutorials |= TutorialStep.ItemsList;
            this.authService.updateLocalUserTutorial(TutorialStep.ItemsList);
          });
        }, 300);
      }
    });
  }

  goBack(): void {
    if (window.history.length > 1) {
      this.location.back();
    } else {
      this.router.navigate(['/dashboard']);
    }
  }

  private loadInitialData(): void {
    this.categoryService.getAll().subscribe(res => { this.categories = res; });
    this.locationService.getAll().subscribe(res => { this.locations = res; });
  }

  loadData(): void {
    this.isLoading = true;
    this.itemService.getAll(this.filterForm.getRawValue()).pipe(
      finalize(() => { this.isLoading = false; })
    ).subscribe(items => {
      this.items = items;
      this.syncSelection();
    });
  }

  private syncSelection(): void {
    const currentIds = new Set(this.items.map(x => x.id));
    this.selectedIds.forEach(id => {
      if (!currentIds.has(id)) this.selectedIds.delete(id);
    });
  }

  updateFilters(value: Partial<GetItemFilters>): void {
    this.filterForm.patchValue(value);
  }

  toggleDropdown(menu: DropdownType, event: Event): void {
    event.stopPropagation();
    this.activeDropdown = this.activeDropdown === menu ? null : menu;
  }

  closeDropdown(): void {
    this.activeDropdown = null;
  }

  setFilter(field: keyof GetItemFilters, value: GetItemFilters[keyof GetItemFilters]): void {
    this.updateFilters({ [field]: value });
    this.closeDropdown();
  }

  toggleSort(asc: number, desc: number): void {
    const current = this.filterForm.get('sortBy')?.value;
    this.updateFilters({ sortBy: current === asc ? desc : asc });
    this.closeDropdown();
  }

  getSortIcon(asc: number, desc: number): string {
    const current = this.filterForm.get('sortBy')?.value;
    if (current === asc) return 'fa-sort-amount-up active-sort';
    if (current === desc) return 'fa-sort-amount-down active-sort';
    return 'fa-sort muted-sort';
  }

  toggleSelection(id: string): void {
    if (this.selectedIds.has(id)) this.selectedIds.delete(id);
    else this.selectedIds.add(id);
  }

  toggleAll(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      this.items
        .filter(item => item.status !== 2)
        .forEach(item => this.selectedIds.add(item.id));
      return;
    }
    this.selectedIds.clear();
  }

  isAllSelected(): boolean {
    const selectableItems = this.items.filter(item => item.status !== 2);
    return selectableItems.length > 0 && this.selectedIds.size === selectableItems.length;
  }

  resetFilters(): void {
    this.filterForm.reset({
      searchQuery: '', categoryId: null, storageLocationId: null, status: null,
      sortBy: 0, minPrice: null, maxPrice: null, showArchived: false, includeArchived: false
    });
  }

  // --- МАССОВОЕ ДЕЙСТВИЕ (БАТЧ) ---
  bulkDelete(): void {
    if (this.selectedIds.size === 0) return;

    const selectedItems = this.items.filter(x => this.selectedIds.has(x.id));
    if (selectedItems.some(x => x.status === 2)) {
      this.toastr.error(this.translate.instant('ITEMS.ERRORS.CANNOT_DELETE_SOLD'));
      return;
    }

    const isArchiveView = this.filterForm.get('showArchived')?.value === true;
    const isHardDelete = isArchiveView || selectedItems.every(x => x.status === 3);

    const confirmTitle = isHardDelete ? 'COMMON.HARD_DELETE' : 'COMMON.ARCHIVE';
    const confirmMessageKey = isHardDelete 
      ? 'ITEMS_LIST.BULK_HARD_DELETE_CONFIRM' 
      : 'ITEMS_LIST.BULK_DELETE_COUNT_CONFIRM';

    this.modal.openConfirm({
      mode: 'delete',
      title: confirmTitle,
      message: this.translate.instant(confirmMessageKey, { count: this.selectedIds.size })
    }).subscribe(res => {
      if (!res) return;

      this.isLoading = true;
      const ids = Array.from(this.selectedIds);
      const request$ = isHardDelete 
        ? this.itemService.deleteArchivedBatch(ids) 
        : this.itemService.deleteBatch(ids);

      request$.subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('ITEMS.SUCCESS.DELETE'));
          this.selectedIds.clear();
          this.loadData();
        },
        error: () => {
          this.toastr.error(this.translate.instant('SYSTEM.DEFAULT_ERROR'));
          this.isLoading = false;
        }
      });
    });
  }

  onEditClick(item: Item): void {
    if (item.status !== 0) {
      this.toastr.warning(this.translate.instant('ITEMS.ERRORS.ONLY_ACTIVE_CAN_BE_EDITED'));
      return;
    }

    this.modal.openItemForm(item).subscribe(res => {
      if (res) this.loadData();
    });
  }

  // --- ОДИНОЧНОЕ УДАЛЕНИЕ ИЗ ТАБЛИЦЫ ---
  onDeleteClick(item: Item): void {
    if (item.status === 2) {
      this.toastr.error(this.translate.instant('ITEMS.ERRORS.CANNOT_DELETE_SOLD'));
      return;
    }

    const isArchived = item.status === 3;
    const confirmTitle = isArchived ? 'COMMON.HARD_DELETE' : 'COMMON.ARCHIVE';
    const confirmMessageKey = isArchived 
      ? 'ITEMS_LIST.HARD_DELETE_CONFIRM' 
      : 'ITEMS_LIST.ARCHIVE_CONFIRM';

    this.modal.openConfirm({
      mode: 'delete',
      title: confirmTitle,
      message: this.translate.instant(confirmMessageKey, { name: item.name }),
      name: item.name
    }).subscribe(confirm => {
      if (!confirm) return;

      this.isLoading = true;

      const request$ = isArchived
        ? this.itemService.deleteArchivedBatch([item.id])
        : this.itemService.changeStatus(item.id, 3);

      request$.subscribe({
        next: () => {
          this.toastr.success(this.translate.instant(isArchived ? 'ITEMS.SUCCESS.DELETE' : 'ITEMS.SUCCESS.ARCHIVE'));
          this.selectedIds.delete(item.id);
          this.loadData();
        },
        error: () => {
          this.toastr.error(this.translate.instant('SYSTEM.DEFAULT_ERROR'));
          this.isLoading = false;
        }
      });
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.th-dropdown-panel') && !target.closest('.filterable') && !target.closest('.sort-btn')) {
      this.closeDropdown();
    }
  }
}