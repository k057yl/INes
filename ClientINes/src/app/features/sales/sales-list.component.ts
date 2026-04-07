import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { SalesService } from '../../shared/services/sales.service';
import { SaleResponseDto } from '../../models/dtos/sale.dto';
import { SaleCardComponent } from '../../shared/components/sale-card/sale-card.component';
import { InestModalComponent } from '../../shared/components/modal/shared-modal/inest-modal.component';
import { finalize } from 'rxjs';

export type SalesAction = 'undo' | 'delete' | null;

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [
    CommonModule, 
    TranslateModule, 
    SaleCardComponent, 
    InestModalComponent
  ], 
  templateUrl: './sales-list.component.html',
  styleUrl: './sales-list.component.scss'
})
export class SalesListComponent implements OnInit {
  private salesService = inject(SalesService);

  sales: SaleResponseDto[] = [];
  isLoading = true;
  readonly EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

  activeAction: SalesAction = null;
  selectedSale: SaleResponseDto | null = null;

  get totalRevenue(): number {
    return this.sales.reduce((acc, curr) => acc + (curr.salePrice || 0), 0);
  }

  get totalProfit(): number {
    return this.sales.reduce((acc, curr) => acc + (curr.profit || 0), 0);
  }

  ngOnInit() {
    this.loadHistory();
  }

  loadHistory() {
    this.isLoading = true;
    this.salesService.getHistory()
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (data: SaleResponseDto[]) => this.sales = data,
        error: (err) => console.error('Error fetching sales', err)
      });
  }

  handleUndo(sale: SaleResponseDto) {
    this.selectedSale = sale;
    this.activeAction = 'undo';
  }

  handleDelete(sale: SaleResponseDto) {
    this.selectedSale = sale;
    this.activeAction = 'delete';
  }

  onConfirm(result?: any) {
    if (!this.selectedSale || !this.activeAction) return;

    if (this.activeAction === 'undo') {
      this.executeUndo(this.selectedSale);
    } else if (this.activeAction === 'delete') {
      const keepHistory = result === 'smart';
      this.executeDelete(this.selectedSale, keepHistory);
    }
    this.closeModal();
  }

  private executeUndo(sale: SaleResponseDto) {
    this.isLoading = true;
    this.salesService.cancelSale(sale.itemId)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: () => {
          this.sales = this.sales.filter(s => s.saleId !== sale.saleId);
        },
        error: (err) => {
          console.error('Undo failed', err);
          if (err.status === 404) {
            this.sales = this.sales.filter(s => s.saleId !== sale.saleId);
          }
        }
      });
  }

  private executeDelete(sale: SaleResponseDto, keepHistory: boolean) {
    this.isLoading = true;
    this.salesService.smartDelete(sale.saleId, keepHistory)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: () => {
          if (keepHistory) {
            this.sales = this.sales.map(s => 
              s.saleId === sale.saleId 
                ? { ...s, itemId: this.EMPTY_GUID } 
                : s
            );
          } else {
            this.sales = this.sales.filter(s => s.saleId !== sale.saleId);
          }
        },
        error: (err) => {
          console.error('Delete failed', err);
          if (err.status === 404) {
            this.sales = this.sales.filter(s => s.saleId !== sale.saleId);
          }
        }
      });
  }

  closeModal() {
    this.activeAction = null;
    this.selectedSale = null;
  }
}