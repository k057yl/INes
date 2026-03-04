import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { SalesService } from '../../shared/services/sales.service';
import { SaleResponseDto } from '../../models/dtos/sale.dto';
import { SaleCardComponent } from '../../shared/components/sale-card/sale-card.component';

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, SaleCardComponent], 
  templateUrl: './sales-list.component.html',
  styleUrl: './sales-list.component.scss'
})
export class SalesListComponent implements OnInit {
  private salesService = inject(SalesService);
  sales: SaleResponseDto[] = [];

  get totalRevenue(): number {
    return this.sales.reduce((acc, curr) => acc + curr.salePrice, 0);
  }

  get totalProfit(): number {
    return this.sales.reduce((acc, curr) => acc + curr.profit, 0);
  }

  ngOnInit() {
    this.loadHistory();
  }

  loadHistory() {
    this.salesService.getHistory().subscribe({
      next: (data: SaleResponseDto[]) => this.sales = data,
      error: (err) => console.error('Error fetching sales', err)
    });
  }

  handleUndo(sale: SaleResponseDto) {
    if (confirm(`Вернуть "${sale.itemName}" в инвентарь?`)) {
      this.salesService.cancelSale(sale.itemId).subscribe({
        next: () => {
          this.sales = this.sales.filter(s => s.saleId !== sale.saleId);
        },
        error: (err: any) => console.error('Undo failed', err)
      });
    }
  }

  handleDelete(event: { sale: SaleResponseDto, keepHistory: boolean }) {
    this.salesService.smartDelete(event.sale.saleId, event.keepHistory).subscribe({
      next: () => {
        this.sales = this.sales.filter(s => s.saleId !== event.sale.saleId);
      },
      error: (err: any) => console.error('Delete failed', err)
    });
  }
}