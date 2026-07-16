import {
  Component,
  HostListener,
  OnInit,
  inject,
  ElementRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import {
  FormBuilder,
  ReactiveFormsModule
} from '@angular/forms';

import {
  debounceTime,
  finalize,
  switchMap,
  tap,
  startWith,
  catchError
} from 'rxjs/operators';
import { of } from 'rxjs';

import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { ItemService } from '../../../../core/services/item.service';
import { CategoryService } from '../../../../core/services/category.service';
import { LocationService } from '../../../../core/services/location.service';

import { Item } from '../../../../core/contracts/item';
import { GetItemFilters } from '../../../../core/dtos/items-get.dto';

import { StatusNamePipe } from '../../../../shared/pipes/status-name.pipe';
import { DashboardModalService } from '../../../dashboard/dashboard.modal.service';

type DropdownType =
  | 'category'
  | 'location'
  | 'status'
  | 'sort'
  | null;

@Component({
  selector: 'app-items-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    ReactiveFormsModule,
    StatusNamePipe
  ],
  templateUrl: './items-list.component.html',
  styleUrl: './items-list.component.scss'
})
export class ItemsListComponent implements OnInit {

  private fb = inject(FormBuilder);

  private itemService = inject(ItemService);
  private categoryService = inject(CategoryService);
  private locationService = inject(LocationService);

  private modalService = inject(DashboardModalService);

  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);

  private eRef = inject(ElementRef);

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

    return !!(
      f.searchQuery ||
      f.categoryId ||
      f.storageLocationId ||
      f.status !== null ||
      f.minPrice !== null ||
      f.maxPrice !== null
    );
  }

  ngOnInit(): void {
    this.loadInitialData();

    this.filterForm.get('showArchived')?.valueChanges.subscribe(val => {
      if (val) {
        this.filterForm.get('includeArchived')?.setValue(false, { emitEvent: false });
      }
    });

    this.filterForm.get('includeArchived')?.valueChanges.subscribe(val => {
      if (val) {
        this.filterForm.get('showArchived')?.setValue(false, { emitEvent: false });
      }
    });

    this.filterForm.valueChanges.pipe(
      startWith(this.filterForm.getRawValue()),
      debounceTime(300),
      tap(() => {
        this.isLoading = true; 
      }),
      switchMap(filters => {
        const typedFilters = filters as GetItemFilters;
        
        return this.itemService.getAll(typedFilters).pipe(
          catchError((err) => {
            console.error('Ошибка бэкенда при поиске:', err);
            return of([]); 
          }),
          finalize(() => {
            this.isLoading = false;
          })
        );
      })
    ).subscribe(items => {
      this.items = items;
      this.syncSelection();
    });
  }

  private loadInitialData(): void {

    this.categoryService
      .getAll()
      .subscribe(res => {
        this.categories = res;
      });

    this.locationService
      .getAll()
      .subscribe(res => {
        this.locations = res;
      });
  }

  loadData(): void {

    this.isLoading = true;

    const filters = this.filterForm.getRawValue();

    this.itemService
      .getAll(filters)
      .pipe(
        finalize(() => {
          this.isLoading = false;
        })
      )
      .subscribe(items => {
        this.items = items;
        this.syncSelection();
      });
  }

  private syncSelection(): void {

    const currentIds = new Set(
      this.items.map(x => x.id)
    );

    this.selectedIds.forEach(id => {

      if (!currentIds.has(id)) {
        this.selectedIds.delete(id);
      }
    });
  }

  updateFilters(value: Partial<GetItemFilters>): void {
    this.filterForm.patchValue(value);
  }

  toggleDropdown(
    menu: DropdownType,
    event: Event
  ): void {

    event.stopPropagation();

    this.activeDropdown =
      this.activeDropdown === menu
        ? null
        : menu;
  }

  closeDropdown(): void {
    this.activeDropdown = null;
  }

  setFilter(field: keyof GetItemFilters, value: GetItemFilters[keyof GetItemFilters]): void {
    this.updateFilters({
      [field]: value
    });

    this.closeDropdown();
  }

  toggleSort(
    asc: number,
    desc: number
  ): void {

    const current =
      this.filterForm.get('sortBy')?.value;

    const next =
      current === asc
        ? desc
        : asc;

    this.updateFilters({
      sortBy: next
    });

    this.closeDropdown();
  }

  getSortIcon(
    asc: number,
    desc: number
  ): string {

    const current =
      this.filterForm.get('sortBy')?.value;

    if (current === asc) {
      return 'fa-sort-amount-up active-sort';
    }

    if (current === desc) {
      return 'fa-sort-amount-down active-sort';
    }

    return 'fa-sort muted-sort';
  }

  toggleSelection(id: string): void {

    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
  }

  toggleAll(event: Event): void {

    const checked =
      (event.target as HTMLInputElement).checked;

    if (checked) {

      this.items.forEach(item => {
        this.selectedIds.add(item.id);
      });

      return;
    }

    this.selectedIds.clear();
  }

  isAllSelected(): boolean {

    return (
      this.items.length > 0 &&
      this.selectedIds.size === this.items.length
    );
  }

  resetFilters(): void {
    this.filterForm.reset({
      searchQuery: '',
      categoryId: null,
      storageLocationId: null,
      status: null,
      sortBy: 0,
      minPrice: null,
      maxPrice: null,
      showArchived: false,
      includeArchived: false
    });
  }

  bulkDelete(): void {

    if (this.selectedIds.size === 0) {
      return;
    }

    const hasSoldItems = this.items
      .filter(x => this.selectedIds.has(x.id))
      .some(x => x.status === 2);

    if (hasSoldItems) {
      this.toastr.error(
        this.translate.instant(
          'ITEMS.ERRORS.CANNOT_DELETE_SOLD'
        )
      );
      return;
    }

    const message = this.translate.instant(
      'ITEMS_LIST.BULK_DELETE_COUNT_CONFIRM',
      {
        count: this.selectedIds.size
      }
    );

    this.modalService.openConfirm({
      mode: 'delete',
      title: 'COMMON.DELETE',
      message
    })
    .subscribe(res => {

      if (!res) {
        return;
      }

      this.isLoading = true;

      this.itemService
        .deleteBatch(
          Array.from(this.selectedIds)
        )
        .subscribe({

          next: () => {

            this.toastr.success(
              this.translate.instant(
                'ITEMS.SUCCESS.DELETE'
              )
            );

            this.loadData();
          },

          error: () => {

            this.toastr.error(
              this.translate.instant(
                'SYSTEM.DEFAULT_ERROR'
              )
            );

            this.isLoading = false;
          }
        });
    });
  }

  onEditClick(item: Item): void {

    if (item.status !== 0) {

      this.toastr.warning(
        this.translate.instant(
          'ITEMS.ERRORS.ONLY_ACTIVE_CAN_BE_EDITED'
        )
      );

      return;
    }

    this.modalService
      .openItemForm(item)
      .subscribe(res => {

        if (res) {
          this.loadData();
        }
      });
  }

  onDeleteClick(item: Item): void {

    this.selectedIds.clear();

    this.selectedIds.add(item.id);

    this.bulkDelete();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;

    if (
      !target.closest('.th-dropdown-panel') && 
      !target.closest('.filterable') && 
      !target.closest('.sort-btn')
    ) {
      this.closeDropdown();
    }
  }
}