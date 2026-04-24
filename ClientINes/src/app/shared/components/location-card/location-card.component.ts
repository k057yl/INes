import { Component, Input, Output, EventEmitter, inject, ElementRef, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DragDropModule, CdkDragDrop, CdkDrag } from '@angular/cdk/drag-drop'; 
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { Item } from '../../../models/entities/item.entity';
import { DashboardModalService } from '../../../features/dashboard/dashboard.modal.service';
import { DashboardFacade } from '../../../features/dashboard/dashboard.facade';
import { ItemCardComponent } from '../item-card/item-card.component';

@Component({
  selector: 'app-location-card',
  standalone: true,
  imports: [CommonModule, DragDropModule, RouterModule, ItemCardComponent, TranslateModule],
  templateUrl: './location-card.component.html',
  styleUrl: './location-card.component.scss'
})
export class LocationCardComponent {
  private el = inject(ElementRef);
  public modalService = inject(DashboardModalService);
  public facade = inject(DashboardFacade);

  @Input({ required: true }) location!: StorageLocation;
  @Input() accentColor?: string;
  @Input() flatLocations: StorageLocation[] = [];
  @Input() connectedLists: string[] = [];
  @Input() connectedLocationLists: string[] = [];
  @Input() isChildOf!: (targetId: string, sourceLoc: StorageLocation) => boolean;

  @Output() itemDropped = new EventEmitter<{event: CdkDragDrop<Item[]>, loc: StorageLocation}>();
  @Output() locationDropped = new EventEmitter<{event: CdkDragDrop<StorageLocation[]>, targetId: string | null}>();
  @Output() move = new EventEmitter<{ loc: StorageLocation, targetId: string }>();
  @Output() rename = new EventEmitter<StorageLocation>();
  @Output() delete = new EventEmitter<StorageLocation>();
  @Output() sellItem = new EventEmitter<Item>();
  @Output() lendItem = new EventEmitter<Item>();
  @Output() deleteItem = new EventEmitter<Item>();
  @Output() moveItemManual = new EventEmitter<{item: Item, targetLocationId: string}>();
  @Output() moveUp = new EventEmitter<StorageLocation>();
  @Output() moveDown = new EventEmitter<StorageLocation>();
  @Output() editItem = new EventEmitter<Item>();

  openItemMenuId: string | null = null;
  isMobile = window.innerWidth <= 768;

  isDragOver = false;

  get effectiveColor(): string {
    return this.accentColor || this.location.color || 'var(--accent-color)';
  }

  onDragEntered() {
    this.isDragOver = true;
  }
  onDragExited() {
    this.isDragOver = false;
  }

  ngOnInit() {
    if (!this.location.children) {
      this.location.children = [];
    }
  }

  onDragStart(child: StorageLocation) {
    this.facade.draggedLocationId = child.id;
  }

  onDragEnd() {
    this.facade.draggedLocationId = null;
  }

  private _checkIfValidTarget(draggedData: any): boolean {
    if (!draggedData || !('children' in draggedData)) return false;
    if (draggedData.id === this.location.id) return false;
    if (this.facade.isChildOf(this.location.id, draggedData)) return false;
    return this.facade.canMoveLocation(draggedData.id, this.location.id);
  }

  @HostListener('window:resize')
  onResize() { this.isMobile = window.innerWidth <= 768; }

  canDropItem = (drag: CdkDrag): boolean => {
    return drag.data && !('children' in drag.data);
  };

  canDropLocation = (drag: CdkDrag): boolean => {
    return this._checkIfValidTarget(drag.data);
  };

  get isValidDropTarget(): boolean {
    const draggedId = this.facade.draggedLocationId;
    if (!draggedId) return false;
    const draggedLoc = this.facade.flatLocations.find(l => l.id === draggedId);
    if (!draggedLoc) return false;
    return this._checkIfValidTarget(draggedLoc);
  }

  get isInvalidDropTarget(): boolean {
    const draggedId = this.facade.draggedLocationId;
    if (!draggedId) return false;
    if (draggedId === this.location.id) return false;
    
    const draggedLoc = this.facade.flatLocations.find(l => l.id === draggedId);
    if (!draggedLoc) return false;
    
    return !this._checkIfValidTarget(draggedLoc);
  }

  private getSiblings(): StorageLocation[] {
    const parent = this.facade.flatLocations.find(l => l.children?.some(c => c.id === this.location.id));
    return parent && parent.children ? parent.children : this.facade.locations;
  }

  get isFirst(): boolean {
    const siblings = this.getSiblings();
    return siblings.length > 0 && siblings[0].id === this.location.id;
  }

  get isLast(): boolean {
    const siblings = this.getSiblings();
    return siblings.length > 0 && siblings[siblings.length - 1].id === this.location.id;
  }

  onItemMenuToggled(itemId: string | null) {
    this.openItemMenuId = itemId;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.location.showMenu && !this.openItemMenuId) return;
    if (!this.el.nativeElement.contains(event.target)) {
      this.location.showMenu = false;
      this.openItemMenuId = null;
    }
  }

  onItemDrop(event: CdkDragDrop<Item[]>) {
    this.itemDropped.emit({ event, loc: this.location });
  }

  toggleMenu(event: MouseEvent) {
    event.stopPropagation();
    this.location.showMenu = !this.location.showMenu;
  }

  onMove(event: Event) {
    const select = event.target as HTMLSelectElement;
    const targetId = select.value === 'root' ? null : select.value;

    if (!this.facade.canMoveLocation(this.location.id, targetId)) {
      select.value = "";
      return;
    }

    this.move.emit({ loc: this.location, targetId: targetId ?? 'root' });
    this.location.showMenu = false;
    select.value = "";
  }

  onAddLocation(event: Event) {
    event.stopPropagation();
    this.modalService.openLocationForm(null, this.location.id).subscribe(result => {
      if (result) {
        this.facade.loadData().subscribe(); 
      }
    });
  }

  onAddItem(event: Event) {
    event.stopPropagation();
    this.modalService.openItemForm(null, this.location.id).subscribe(result => {
      if (result) {
        this.facade.loadData().subscribe();
      }
    });
  }

  trackById(index: number, item: any): string {
    return item.id;
  }
}