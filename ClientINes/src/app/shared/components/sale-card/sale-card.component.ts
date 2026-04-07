import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SaleResponseDto } from '../../../models/dtos/sale.dto';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-sale-card',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './sale-card.component.html',
  styleUrl: './sale-card.component.scss'
})
export class SaleCardComponent {
  @Input() sale!: SaleResponseDto;
  @Output() undo = new EventEmitter<SaleResponseDto>();
  @Output() delete = new EventEmitter<SaleResponseDto>();

  private readonly EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

  get isItemExists(): boolean {
    return !!this.sale.itemId && this.sale.itemId !== this.EMPTY_GUID;
  }

  onUndo() { this.undo.emit(this.sale); }
  onDelete() { this.delete.emit(this.sale); }
}