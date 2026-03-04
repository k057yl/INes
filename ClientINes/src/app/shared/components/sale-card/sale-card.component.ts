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
  @Output() delete = new EventEmitter<{sale: SaleResponseDto, keepHistory: boolean}>();

  showDeleteModal = false;

  onUndo() {
    this.undo.emit(this.sale);
  }

  get isItemExists(): boolean {
    return !!this.sale.itemId && this.sale.itemId !== '00000000-0000-0000-0000-000000000000';
  }

  onDelete() {
    this.showDeleteModal = true;
  }

  confirmDelete(keepHistory: boolean) {
    this.showDeleteModal = false;
    this.delete.emit({ sale: this.sale, keepHistory });
  }

  closeModal() {
    this.showDeleteModal = false;
  }
}