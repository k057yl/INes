import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { SalesService } from '../../shared/services/sales.service';
import { ItemService } from '../../shared/services/item.service';
import { SaleResponseDto } from '../../models/dtos/sale.dto';
import { SaleCardComponent } from '../../shared/components/sale-card/sale-card.component';
import { InestModalComponent } from '../../shared/components/modal/shared-modal/inest-modal.component';
import { finalize } from 'rxjs';

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
  private itemService = inject(ItemService);

  sales: SaleResponseDto[] = [];
  isLoading = true;

  showUndoModal = false;
  saleToUndo: SaleResponseDto | null = null;

  readonly EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

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
    this.saleToUndo = sale;
    this.showUndoModal = true;
  }

  onConfirmUndo() {
    if (!this.saleToUndo) return;

    const sale = this.saleToUndo;
    this.showUndoModal = false;
    this.isLoading = true;
    this.salesService.cancelSale(sale.itemId)
      .pipe(finalize(() => {
        this.isLoading = false;
        this.saleToUndo = null;
      }))
      .subscribe({
        next: () => {
          this.sales = this.sales.filter(s => s.saleId !== sale.saleId);
        },
        error: (err) => console.error('Undo failed', err)
      });
  }

  handleDelete(event: { sale: SaleResponseDto, keepHistory: boolean }) {
    const { sale, keepHistory } = event;
    this.isLoading = true;
    this.salesService.smartDelete(sale.saleId, keepHistory)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: () => {
          if (keepHistory) {
            const index = this.sales.findIndex(s => s.saleId === sale.saleId);
            if (index !== -1) this.sales[index] = { ...sale, itemId: this.EMPTY_GUID };
          } else {
            this.sales = this.sales.filter(s => s.saleId !== sale.saleId);
          }
        },
        error: (err) => console.error('Delete failed', err)
      });
  }
}