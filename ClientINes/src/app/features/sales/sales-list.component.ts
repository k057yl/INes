import { Component, inject, OnInit, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { debounceTime, finalize, switchMap, tap, startWith, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

import { SalesService } from '../../shared/services/sales.service';
import { CategoryService } from '../../shared/services/category.service';
import { SaleResponseDto, SaleFilters } from '../../models/dtos/sale.dto';
import { Platform } from '../../models/entities/platform.entity';
import { Category } from '../../models/entities/category.entity';
import { SaleCardComponent } from '../../shared/components/sale-card/sale-card.component';
import { InestModalComponent } from '../../shared/components/modals/inest-modal/inest-modal.component';

export type SalesAction = 'undo' | 'delete' | null;

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, ReactiveFormsModule, SaleCardComponent, InestModalComponent],
  templateUrl: './sales-list.component.html',
  styleUrl: './sales-list.component.scss'
})
export class SalesListComponent implements OnInit {
  private salesService = inject(SalesService);
  private categoryService = inject(CategoryService);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);
  private fb = inject(FormBuilder);
  private eRef = inject(ElementRef);

  sales: SaleResponseDto[] = [];
  platforms: Platform[] = [];
  categories: Category[] = [];
  isLoading = true;
  readonly EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

  activeAction: SalesAction = null;
  selectedSale: SaleResponseDto | null = null;
  activeDropdown: 'platform' | 'sort' | 'category' | null = null;

  filterForm = this.fb.group({
    searchQuery: [''],
    platformId: [null as string | null],
    categoryId: [null as string | null],
    sortBy: [0],
    minPrice: [null as number | null],
    maxPrice: [null as number | null],
    minProfit: [null as number | null],
    startDate: [null as string | null],
    endDate: [null as string | null]
  });

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.th-dropdown-wrapper')) {
      this.activeDropdown = null;
    }
  }

  get totalRevenue(): number { return this.sales.reduce((acc, curr) => acc + (curr.salePrice || 0), 0); }
  get totalProfit(): number { return this.sales.reduce((acc, curr) => acc + (curr.profit || 0), 0); }
  
  get hasActiveFilters(): boolean {
    const f = this.filterForm.getRawValue();
    return !!(f.searchQuery || f.platformId || f.categoryId || f.minPrice || f.maxPrice || f.minProfit || f.startDate || f.sortBy !== 0);
  }

  ngOnInit() {
    this.salesService.getPlatforms().subscribe(res => this.platforms = res);
    this.categoryService.getAll().subscribe(res => this.categories = res);

    this.filterForm.valueChanges.pipe(
      startWith(this.filterForm.getRawValue()),
      debounceTime(350),
      tap(() => this.isLoading = true),
      switchMap(filters => this.salesService.getHistory(filters as SaleFilters).pipe(
        catchError(err => {
          console.error('FETCH_ERROR:', err);
          return of([]);
        }),
        finalize(() => this.isLoading = false)
      ))
    ).subscribe(data => this.sales = data);
  }

  setFilter(field: keyof SaleFilters, value: any): void {
    this.filterForm.patchValue({ [field]: value });
    this.activeDropdown = null;
  }

  resetFilters(): void {
    this.filterForm.reset({
      searchQuery: '', platformId: null, categoryId: null, sortBy: 0, 
      minPrice: null, maxPrice: null, minProfit: null, startDate: null, endDate: null
    });
  }

  handleUndo(sale: SaleResponseDto) {
    console.log('UNDO_CLICKED:', sale);
    this.selectedSale = sale;
    this.activeAction = 'undo';
  }

  handleDelete(sale: SaleResponseDto) {
    console.log('DELETE_CLICKED:', sale);
    this.selectedSale = sale;
    this.activeAction = 'delete';
  }

  onConfirm(result?: any) {
    if (!this.selectedSale || !this.activeAction) return;

    if (this.activeAction === 'undo') {
      this.executeUndo(this.selectedSale);
    } else if (this.activeAction === 'delete') {
      this.executeDelete(this.selectedSale, result === 'smart');
    }
    this.closeModal();
  }

  private executeUndo(sale: SaleResponseDto) {
    this.isLoading = true;
    this.salesService.cancelSale(sale.itemId)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(() => {
        this.toastr.success(this.translate.instant('SALES.SUCCESS.CANCEL'));
        this.refreshData();
      });
  }

  private executeDelete(sale: SaleResponseDto, keepHistory: boolean) {
    this.isLoading = true;
    this.salesService.smartDelete(sale.saleId, keepHistory)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(() => {
        this.toastr.success(this.translate.instant('SALES.SUCCESS.DELETE'));
        this.refreshData();
      });
  }

  refreshData() {
    this.salesService.getHistory(this.filterForm.getRawValue() as SaleFilters)
      .subscribe(data => this.sales = data);
  }

  closeModal() {
    this.activeAction = null;
    this.selectedSale = null;
  }
}