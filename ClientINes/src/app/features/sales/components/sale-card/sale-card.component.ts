import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { SaleListItem } from '../../contracts/sale-list-item';
import { PricePipe } from '../../../../shared/pipes/price-currency.pipe';

@Component({
  selector: 'app-sale-card',
  standalone: true,
  imports: [CommonModule, TranslateModule, PricePipe],
  templateUrl: './sale-card.component.html',
  styleUrl: './sale-card.component.scss'
})
export class SaleCardComponent {
  @Input() sale!: SaleListItem;
  @Output() undo = new EventEmitter<SaleListItem>();
  @Output() delete = new EventEmitter<SaleListItem>();

  private readonly EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

  get isItemExists(): boolean {
    return !!this.sale.itemId && this.sale.itemId !== this.EMPTY_GUID;
  }

  onUndo(): void { 
    this.undo.emit(this.sale); 
  }

  onDelete(): void { 
    this.delete.emit(this.sale); 
  }
}