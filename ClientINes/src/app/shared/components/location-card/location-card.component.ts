import { Component, Input, Output, EventEmitter, inject, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DragDropModule, CdkDragDrop } from '@angular/cdk/drag-drop';
import { RouterModule } from '@angular/router';
import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { Item } from '../../../models/entities/item.entity';
import { FeatureService } from '../../../core/services/feature.service';
import { ItemCardComponent } from '../item-card/item-card.component';
import { TranslateModule } from '@ngx-translate/core';
import { ColorChromeModule } from 'ngx-color/chrome';

@Component({
  selector: 'app-location-card',
  standalone: true,
  imports: [CommonModule, DragDropModule, RouterModule, ItemCardComponent, TranslateModule, ColorChromeModule],
  templateUrl: './location-card.component.html',
  styleUrl: './location-card.component.scss'
})
export class LocationCardComponent {
  private el = inject(ElementRef);
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
  @Output() moveItemManual = new EventEmitter<{item: Item, targetLocationId: string}>();
  @Output() lendItem = new EventEmitter<Item>();

  openItemMenuId: string | null = null;

  onItemMenuToggled(itemId: string | null) {
    this.openItemMenuId = itemId;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.location.showMenu && !this.openItemMenuId) return;

    const target = event.target as HTMLElement;

    if (!this.el.nativeElement.contains(target)) {
      this.location.showMenu = false;
      this.openItemMenuId = null;
    }
  }

  onItemMoveManual(data: {item: Item, targetLocationId: string}) {
    this.moveItemManual.emit(data);
  }

  onItemDrop(event: CdkDragDrop<Item[]>) {
    this.itemDropped.emit({ event, loc: this.location });
  }

  onMove(event: Event) {
    const targetId = (event.target as HTMLSelectElement).value;
    if (targetId) {
      this.move.emit({ loc: this.location, targetId });
      this.location.showMenu = false;
    }
  }
}