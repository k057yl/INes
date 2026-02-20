import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { Item } from '../../models/entities/item.entity';
import { FeatureService } from '../../services/feature.service';

@Component({
  selector: 'app-item-card',
  standalone: true,
  imports: [CommonModule, RouterModule, DragDropModule],
  templateUrl: './item-card.component.html',
  styleUrl: './item-card.component.css'
})
export class ItemCardComponent {
  public featureService = inject(FeatureService);

  @Input({ required: true }) item!: Item;
  
  @Output() sell = new EventEmitter<Item>();
  @Output() delete = new EventEmitter<Item>();

  onSell(event: MouseEvent) {
    event.stopPropagation();
    this.sell.emit(this.item);
  }

  onDelete(event: MouseEvent) {
    event.stopPropagation();
    this.delete.emit(this.item);
  }
}