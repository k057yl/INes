import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SalesService } from '../../services/sales.service';
import { SaleResponseDto } from '../../models/dtos/sale.dto';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="sales-page">
      <header class="sales-header">
        <h1>{{ 'SALES.TITLE' | translate }}</h1>
        <div class="stats-cards">
          <div class="stat-card">
            <span class="label">{{ 'SALES.TOTAL_REVENUE' | translate }}</span>
            <span class="value turquoise">{{ totalRevenue | number:'1.2-2' }} $</span>
          </div>
          <div class="stat-card">
            <span class="label">{{ 'SALES.TOTAL_PROFIT' | translate }}</span>
            <span class="value" [class.turquoise]="totalProfit >= 0" [class.red]="totalProfit < 0">
              {{ totalProfit | number:'1.2-2' }} $
            </span>
          </div>
        </div>
      </header>

      <div class="table-container">
        <table class="sales-table">
          <thead>
            <tr>
              <th>{{ 'SALES.COL_ITEM' | translate }}</th>
              <th>{{ 'SALES.COL_DATE' | translate }}</th>
              <th>{{ 'SALES.COL_PRICE' | translate }}</th>
              <th>{{ 'SALES.COL_PROFIT' | translate }}</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let sale of sales">
              <td>
                <div class="item-name">{{ sale.itemName }}</div> 
                <div class="platform-hint" *ngIf="sale.platformName">{{ sale.platformName }}</div>
              </td>
              <td class="date-cell">{{ sale.soldDate | date:'dd.MM.yyyy' }}</td>
              <td class="price-cell">{{ sale.salePrice | number:'1.2-2' }} $</td>
              <td class="profit-cell" [ngClass]="sale.profit >= 0 ? 'turquoise' : 'red'">
                {{ sale.profit > 0 ? '+' : '' }}{{ sale.profit | number:'1.2-2' }} $
              </td>
            </tr>
            <tr *ngIf="sales.length === 0">
              <td colspan="4" class="empty-msg">{{ 'SALES.NO_SALES' | translate }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .sales-page { padding: 40px; background: #0b132b; min-height: 100vh; color: white; }
    .sales-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 40px; }
    h1 { font-size: 2rem; font-weight: 800; color: white; margin: 0; }
    .stats-cards { display: flex; gap: 20px; }
    .stat-card { background: #1c2541; padding: 15px 25px; border-radius: 12px; border: 1px solid #3a506b; display: flex; flex-direction: column; min-width: 150px; }
    .stat-card .label { font-size: 0.75rem; color: #94a3b8; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 5px; }
    .stat-card .value { font-size: 1.5rem; font-weight: 700; }
    .table-container { background: #1c2541; border-radius: 16px; border: 1px solid #3a506b; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.3); }
    .sales-table { width: 100%; border-collapse: collapse; }
    th { background: rgba(58, 80, 107, 0.4); padding: 18px 20px; text-align: left; font-size: 0.85rem; color: #94a3b8; text-transform: uppercase; border-bottom: 1px solid #3a506b; }
    td { padding: 18px 20px; border-bottom: 1px solid rgba(58, 80, 107, 0.3); font-size: 1rem; }
    tr:last-child td { border-bottom: none; }
    tr:hover { background: rgba(0, 245, 212, 0.03); }
    .item-name { font-weight: 600; color: #e2e8f0; }
    .platform-hint { font-size: 0.8rem; color: #94a3b8; }
    .date-cell { color: #94a3b8; }
    .price-cell { font-weight: 500; }
    .turquoise { color: #00f5d4 !important; }
    .red { color: #ff4d4d !important; }
    .empty-msg { text-align: center; padding: 40px; color: #94a3b8; font-style: italic; }
  `]
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
    this.salesService.getHistory().subscribe({
      next: (data) => this.sales = data,
      error: (err) => console.error('Error fetching sales', err)
    });
  }
}