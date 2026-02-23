import { Component, Input, Output, EventEmitter, inject, HostListener } from '@angular/core';//****** */
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { Item } from '../../models/entities/item.entity';
import { FeatureService } from '../../services/feature.service';

import { StorageLocation } from '../../models/entities/storage-location.entity';//******* */

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
  @Input() flatLocations: StorageLocation[] = []; // Список для перемещения
  
  @Output() sell = new EventEmitter<Item>();
  @Output() delete = new EventEmitter<Item>();

  @Output() move = new EventEmitter<{item: Item, targetLocationId: string}>();

  get availableLocations(): StorageLocation[] {
    return this.flatLocations.filter(loc => loc.id !== this.item.storageLocationId);
  }

  showMenu = false;
  isMobile = window.innerWidth <= 768; // Простая проверка на мобилку

  @HostListener('window:resize')
  onResize() {
    this.isMobile = window.innerWidth <= 768;
  }

  toggleMenu(event: MouseEvent) {
    event.stopPropagation();
    this.showMenu = !this.showMenu;
  }

  onMove(event: Event) {
    const targetId = (event.target as HTMLSelectElement).value;
    if (targetId) {
      this.move.emit({ item: this.item, targetLocationId: targetId });
      this.showMenu = false;
    }
  }//************** */

  onSell(event: MouseEvent) {
    event.stopPropagation();
    this.sell.emit(this.item);
  }

  onDelete(event: MouseEvent) {
    event.stopPropagation();
    this.delete.emit(this.item);
  }
}