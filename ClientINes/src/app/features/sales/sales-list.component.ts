import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SalesService } from '../../shared/services/sales.service';
import { SaleResponseDto } from '../../models/dtos/sale.dto';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [CommonModule, TranslateModule],
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
      next: (data) => this.sales = data,
      error: (err) => console.error('Error fetching sales', err)
    });
  }

  onCancelSale(sale: SaleResponseDto) {
    if (!sale.itemId) {
      alert('Невозможно отменить: предмет был удален из инвентаря.');
      return;
    }

    if (confirm(`Отменить продажу "${sale.itemName}"? Предмет снова станет активным.`)) {
      this.salesService.cancelSale(sale.itemId).subscribe({
        next: () => {
          this.sales = this.sales.filter(s => s.saleId !== sale.saleId);
        },
        error: (err) => console.error('Failed to cancel sale', err)
      });
    }
  }

  onDeletePermanent(sale: SaleResponseDto) {
  if (confirm(`Внимание! Это полностью удалит предмет и запись о продаже. Продолжить?`)) {
    this.salesService.deletePermanent(sale.saleId).subscribe({
      next: () => {
        this.sales = this.sales.filter(s => s.saleId !== sale.saleId);
      },
      error: (err: any) => {
        console.error('Failed to delete sale permanently', err);
        alert('Не удалось выполнить полное удаление.');
      }
    });
  }
}
}