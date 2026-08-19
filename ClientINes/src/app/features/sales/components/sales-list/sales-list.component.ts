import { Component, inject, OnInit, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormBuilder, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { debounceTime, finalize, switchMap, tap, startWith, catchError, take } from 'rxjs/operators';
import { of } from 'rxjs';
import { Router } from '@angular/router';
import { PricePipe } from '../../../../shared/pipes/price-currency.pipe';

import { SalesService } from '../../services/sales.service';
import { CategoryService } from '../../../category/services/category.service';
import { LocationService } from '../../../location/services/location.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { AuthService } from '../../../auth/services/auth.service';
import { TutorialService, TutorialStep } from '../../../../core/services/tutorial.service';

import { SaleListItem } from '../../contracts/sale-list-item';
import { GetSalesDto } from '../../dtos/sales-get.dto';
import { Platform } from '../../../platform/contracts/platform';
import { Category } from '../../../category/contracts/category';

import { SaleCardComponent } from '../sale-card/sale-card.component';
import { InestModalComponent } from '../../../../shared/components/inest-modal/inest-modal.component';

export type SalesAction = 'undo' | null;

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, ReactiveFormsModule, FormsModule, SaleCardComponent, InestModalComponent, PricePipe],
  templateUrl: './sales-list.component.html',
  styleUrl: './sales-list.component.scss'
})
export class SalesListComponent implements OnInit {
  protected readonly Math = Math;

  private salesService = inject(SalesService);
  private categoryService = inject(CategoryService);
  private locationService = inject(LocationService);
  private modalService = inject(DashboardModalService);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);
  private fb = inject(FormBuilder);
  private eRef = inject(ElementRef);
  private router = inject(Router);

  private authService = inject(AuthService);
  private tutorialService = inject(TutorialService);

  sales: SaleListItem[] = [];
  platforms: Platform[] = [];
  categories: Category[] = [];
  locations: any[] = [];
  selectedReturnLocationId: string | null = null;
  isLoading = true;

  // --- ПАГИНАЦИЯ ---
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  readonly PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

  activeAction: SalesAction = null;
  selectedSale: SaleListItem | null = null;
  activeDropdown: 'platform' | 'sort' | 'category' | null = null;

  revenueByCurrency: Record<string, number> = {};
  profitByCurrency: Record<string, number> = {};

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

  get hasActiveFilters(): boolean {
    const f = this.filterForm.getRawValue();
    return !!(f.searchQuery || f.platformId || f.categoryId || f.minPrice || f.maxPrice || f.minProfit || f.startDate || f.sortBy !== 0);
  }

  // --- МЕТОДЫ ПАГИНАЦИИ ---
  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.pageNumber) return;
    this.pageNumber = page;
    this.refreshData();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  changePageSize(size: number): void {
    if (this.pageSize === size) return;
    this.pageSize = size;
    this.pageNumber = 1;
    this.refreshData();
  }

  get pagesArray(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.pageNumber - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  ngOnInit(): void {
    this.salesService.getPlatforms().subscribe(res => this.platforms = res);
    this.categoryService.getAll().subscribe(res => this.categories = res);
    this.locationService.getAll().subscribe((res: any[]) => this.locations = res);

    this.filterForm.valueChanges.subscribe(() => {
      this.pageNumber = 1;
    });

    this.filterForm.valueChanges.pipe(
      startWith(this.filterForm.getRawValue()),
      debounceTime(350),
      tap(() => {
        this.isLoading = true;
      }),
      switchMap(filters => {
        const queryParams: GetSalesDto = {
          ...filters as GetSalesDto,
          pageNumber: this.pageNumber,
          pageSize: this.pageSize
        };
        return this.salesService.getHistory(queryParams).pipe(
          catchError(err => {
            console.error('FETCH_ERROR:', err);
            return of({ items: [], totalCount: 0, totalPages: 0, pageNumber: 1, pageSize: 10 });
          }),
          finalize(() => this.isLoading = false)
        );
      })
    ).subscribe((res: any) => {
      if (Array.isArray(res)) {
        this.sales = res;
        this.totalCount = res.length;
        this.totalPages = 1;
      } else {
        this.sales = res.items || [];
        this.totalCount = res.totalCount || 0;
        this.totalPages = res.totalPages || Math.ceil(this.totalCount / this.pageSize);
      }
      this.calculateCurrencyTotals();
      this.checkAndStartTutorial();
    });
  }

  private checkAndStartTutorial(): void {
    this.authService.user$.pipe(take(1)).subscribe(user => {
      if (!user) return;

      const completed = user.completedTutorials;
      const isSalesPassed = (completed & TutorialStep.Sales) === TutorialStep.Sales;
      const isFirstSalePassed = (completed & TutorialStep.FirstSaleCard) === TutorialStep.FirstSaleCard;

      if (!isSalesPassed) {
        setTimeout(() => {
          this.tutorialService.startSalesListTour(() => {
            user.completedTutorials |= TutorialStep.Sales;
            this.authService.updateLocalUserTutorial(TutorialStep.Sales);

            if (!isFirstSalePassed && this.sales.length === 1) {
              setTimeout(() => {
                this.tutorialService.startFirstSaleCardTour(() => {
                  user.completedTutorials |= TutorialStep.FirstSaleCard;
                  this.authService.updateLocalUserTutorial(TutorialStep.FirstSaleCard);
                });
              }, 400);
            }
          });
        }, 300);
        return;
      }

      if (!isFirstSalePassed && this.sales.length === 1) {
        setTimeout(() => {
          this.tutorialService.startFirstSaleCardTour(() => {
            user.completedTutorials |= TutorialStep.FirstSaleCard;
            this.authService.updateLocalUserTutorial(TutorialStep.FirstSaleCard);
          });
        }, 400);
      }
    });
  }

  private calculateCurrencyTotals(): void {
    this.revenueByCurrency = {};
    this.profitByCurrency = {};

    for (const sale of this.sales) {
      const curr = sale.currency || 'USD';
      this.revenueByCurrency[curr] = (this.revenueByCurrency[curr] || 0) + (sale.salePrice || 0);
      this.profitByCurrency[curr] = (this.profitByCurrency[curr] || 0) + (sale.profit || 0);
    }
  }

  setFilter(field: keyof GetSalesDto, value: any): void {
    this.filterForm.patchValue({ [field]: value });
    this.activeDropdown = null;
  }

  resetFilters(): void {
    this.pageNumber = 1;
    this.filterForm.reset({
      searchQuery: '', platformId: null, categoryId: null, sortBy: 0, 
      minPrice: null, maxPrice: null, minProfit: null, startDate: null, endDate: null
    });
  }

  handleUndo(sale: SaleListItem): void {
    this.selectedSale = sale;
    this.activeAction = 'undo';
  }

  handleDelete(sale: SaleListItem): void {
    this.modalService.openConfirm({
      mode: 'delete',
      title: 'SALES_PAGE.MODALS.DELETE_TITLE',
      message: this.translate.instant('SALES_PAGE.MODALS.DELETE_MESSAGE')
    }).subscribe(res => {
      if (res) {
        this.executeDelete(sale);
      }
    });
  }

  onConfirmUndo(): void {
    if (!this.selectedSale) return;

    if (!this.selectedReturnLocationId) {
      this.toastr.error(this.translate.instant('ERRORS.REQUIRED_FIELD'));
      return;
    }

    this.executeUndo(this.selectedSale, this.selectedReturnLocationId);
    this.closeModal();
  }

  private executeUndo(sale: SaleListItem, locationId: string): void {
    this.isLoading = true;
    this.salesService.cancelSale(sale.itemId, locationId)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(() => {
        this.toastr.success(this.translate.instant('SALES.SUCCESS.CANCEL'));
        this.refreshData();
      });
  }

  private executeDelete(sale: SaleListItem): void {
    this.isLoading = true;
    this.salesService.deleteSale(sale.saleId)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('SALES.SUCCESS.DELETE'));
          this.refreshData();
        },
        error: () => {
          this.toastr.error(this.translate.instant('SYSTEM.DEFAULT_ERROR'));
        }
      });
  }

  refreshData(): void {
    this.isLoading = true;
    const filters: GetSalesDto = {
      ...this.filterForm.getRawValue() as GetSalesDto,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.salesService.getHistory(filters)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe((res: any) => {
        if (Array.isArray(res)) {
          this.sales = res;
          this.totalCount = res.length;
          this.totalPages = 1;
        } else {
          this.sales = res.items || [];
          this.totalCount = res.totalCount || 0;
          this.totalPages = res.totalPages || Math.ceil(this.totalCount / this.pageSize);
        }
        this.calculateCurrencyTotals();
      });
  }

  goBack(): void {
    if (window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigate(['/dashboard']);
    }
  }

  closeModal(): void {
    this.activeAction = null;
    this.selectedSale = null;
    this.selectedReturnLocationId = null;
  }
}