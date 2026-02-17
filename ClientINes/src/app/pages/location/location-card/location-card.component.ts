import { Component, Input, Output, EventEmitter, HostListener, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { Item } from '../../../models/entities/item.entity';
import { RouterModule } from '@angular/router';
import { FeatureService } from '../../../services/feature.service';
import { ItemService } from '../../../services/item.service';

@Component({
  selector: 'app-location-card',
  standalone: true,
  imports: [CommonModule, DragDropModule, RouterModule],
  templateUrl: './location-card.component.html',
  styleUrl: './location-card.component.css'
})
export class LocationCardComponent {
  public featureService = inject(FeatureService);

  private elementRef = inject(ElementRef);

  private itemService = inject(ItemService);

  @Input() location!: StorageLocation;
  @Input() flatLocations: StorageLocation[] = [];
  @Input() connectedLists: string[] = [];
  @Input() isChildOf!: (targetId: string, sourceLoc: StorageLocation) => boolean;

  @Output() itemDropped = new EventEmitter<{event: CdkDragDrop<Item[]>, loc: StorageLocation}>();
  @Output() locationReordered = new EventEmitter<{event: CdkDragDrop<StorageLocation[]>, parentId: string | null}>();
  @Output() move = new EventEmitter<{loc: StorageLocation, targetId: string}>();
  @Output() rename = new EventEmitter<StorageLocation>();
  @Output() delete = new EventEmitter<StorageLocation>();
  @Output() sellItem = new EventEmitter<Item>();

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    if (this.location.showMenu && !this.elementRef.nativeElement.contains(event.target)) {
      this.location.showMenu = false;
    }
  }

  onToggleMenu(event: MouseEvent) {
    event.stopPropagation();
    this.location.showMenu = !this.location.showMenu;
  }

  onMove(event: Event) {
    const targetId = (event.target as HTMLSelectElement).value;
    this.move.emit({ loc: this.location, targetId });
    this.location.showMenu = false;
  }

  onRenameClick() {
    this.rename.emit(this.location);
    this.location.showMenu = false;
  }

  onDeleteClick() {
    this.delete.emit(this.location);
    this.location.showMenu = false;
  }

  onItemDrop(event: CdkDragDrop<Item[]>) {
    this.itemDropped.emit({ event, loc: this.location });
  }

  onChildLocationDrop(event: CdkDragDrop<StorageLocation[]>) {
    this.locationReordered.emit({ event, parentId: this.location.id });
  }

  onSellClick(event: MouseEvent, item: Item) {
    event.stopPropagation();
    this.sellItem.emit(item); 
  }

  onDeleteItem(event: MouseEvent, item: Item) {
  event.stopPropagation();

  if (confirm(`Вы уверены, что хотите удалить "${item.name}"?`)) {
    this.itemService.deleteItem(item.id).subscribe({
      next: () => {
        this.location.items = this.location.items.filter(i => i.id !== item.id);
        console.log('Предмет успешно удален');
      },
      error: (err) => {
        alert('Не удалось удалить предмет. Возможно, есть связанные данные.');
        console.error(err);
      }
    });
  }
}
}