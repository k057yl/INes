import { Component, Input, Output, EventEmitter, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DragDropModule, CdkDragDrop } from '@angular/cdk/drag-drop';
import { RouterModule } from '@angular/router';
import { StorageLocation } from '../../models/entities/storage-location.entity';
import { Item } from '../../models/entities/item.entity';
import { FeatureService } from '../../services/feature.service';
import { ItemCardComponent } from '../item-card/item-card.component';

@Component({
  selector: 'app-location-card',
  standalone: true,
  imports: [CommonModule, DragDropModule, RouterModule, ItemCardComponent],
  templateUrl: './location-card.component.html',
  styleUrl: './location-card.component.css'
})
export class LocationCardComponent {
  public featureService = inject(FeatureService);

  @Input({ required: true }) location!: StorageLocation;
  @Input() flatLocations: StorageLocation[] = [];
  @Input() connectedLists: string[] = [];
  @Input() isChildOf!: (targetId: string, sourceLoc: StorageLocation) => boolean;

  @Output() itemDropped = new EventEmitter<{event: CdkDragDrop<Item[]>, loc: StorageLocation}>();
  @Output() sellItem = new EventEmitter<Item>();
  @Output() move = new EventEmitter<{ loc: StorageLocation, targetId: string }>();
  @Output() rename = new EventEmitter<StorageLocation>();
  @Output() delete = new EventEmitter<StorageLocation>();
  @Output() deleteItem = new EventEmitter<Item>();

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.location.showMenu) return;

    const target = event.target as HTMLElement;
    const clickedInside = target.closest('.menu-dropdown') || target.closest('.menu-btn');

    if (!clickedInside) {
      this.location.showMenu = false;
    }
  }

  onItemDrop(event: CdkDragDrop<Item[]>) {
    this.itemDropped.emit({ event, loc: this.location });
  }

  onSellClick(event: MouseEvent, item: Item) {
    event.stopPropagation();
    this.sellItem.emit(item);
  }

  onMove(event: Event) {
    const targetId = (event.target as HTMLSelectElement).value;
    this.move.emit({ loc: this.location, targetId });
  }

  onDeleteItem(event: MouseEvent, item: Item) {
    event.stopPropagation();
    this.deleteItem.emit(item);
    }
}