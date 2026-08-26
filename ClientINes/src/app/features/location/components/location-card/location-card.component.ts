import { Component, Input, Output, EventEmitter, inject, ElementRef, HostListener, HostBinding, OnInit } from '@angular/core';
import { DragDropModule, CdkDragDrop, CdkDrag } from '@angular/cdk/drag-drop'; 
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { StorageLocation } from '../../contracts/storage-location';
import { Item } from '../../../item/contracts/item';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { DashboardFacade } from '../../../dashboard/components/dashboard/dashboard.facade';
import { DashboardTreeService } from '../../../dashboard/services/dashboard-tree.service';
import { ItemCardComponent } from '../../../item/components/item-card/item-card.component';

@Component({
  selector: 'app-location-card',
  standalone: true,
  imports: [DragDropModule, RouterModule, ItemCardComponent, TranslateModule],
  templateUrl: './location-card.component.html',
  styleUrl: './location-card.component.scss'
})
export class LocationCardComponent implements OnInit {
  private el = inject(ElementRef);
  public modalService = inject(DashboardModalService);
  public facade = inject(DashboardFacade);
  public treeService = inject(DashboardTreeService);

  @Input({ required: true }) location!: StorageLocation;
  @Input() accentColor?: string;
  @Input() flatLocations: StorageLocation[] = [];
  @Input() connectedLists: string[] = [];

  @HostBinding('attr.id') get elementId(): string {
    return `location-card-${this.location?.id}`;
  }

  @HostBinding('attr.data-location-id') get dataLocationId(): string {
    return this.location?.id;
  }

  @Output() itemDropped = new EventEmitter<{event: CdkDragDrop<Item[]>, loc: StorageLocation}>();
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
  @Output() returnItem = new EventEmitter<Item>();
  @Output() statusChange = new EventEmitter<{item: Item, newStatus: number}>();

  openItemMenuId: string | null = null;
  isMobile = window.innerWidth <= 768;

  get locationLevel(): number {
    const rawLoc = this.location as any;
    const parentId = rawLoc.parentLocationId || rawLoc.parentId || rawLoc.parentLocation?.id;
    
    if (!parentId) {
      return 0;
    }

    return this.treeService.getLocationLevel(this.facade.locations.flatLocations, this.location.id);
  }

  get selfColor(): string {
    return this.location.color || 'var(--accent-color)';
  }

  get effectiveColor(): string {
    if (this.locationLevel === 0) {
      return this.selfColor;
    }

    const flat = this.facade.locations.flatLocations;
    let currentParentId = this.treeService.getParentId(flat, this.location.id);
    let rootColor = this.selfColor;

    while (currentParentId) {
      const parent = flat.find(l => l.id === currentParentId);
      if (parent) {
        rootColor = parent.color || 'var(--accent-color)';
        currentParentId = this.treeService.getParentId(flat, parent.id);
      } else {
        break;
      }
    }

    return rootColor;
  }

  get cardBackgroundStyle(): string {
    if (this.locationLevel === 0) {
      return 'var(--bg-card)';
    }
    return `color-mix(in srgb, ${this.selfColor} 10%, var(--bg-card))`;
  }

  get isNestedLocation(): boolean {
    return this.locationLevel > 0;
  }

  get isMaxLevelReached(): boolean {
    return this.locationLevel >= 3;
  }

  ngOnInit() {
    if (!this.location.children) {
      this.location.children = [];
    }
  }

  onMoveTarget(targetId: string | null, event: Event) {
    this.move.emit({ loc: this.location, targetId: targetId ?? 'root' });

    const target = event.target as HTMLElement;
    const details = target.closest('details');
    if (details) {
      details.removeAttribute('open');
    }
  }

  @HostListener('window:resize')
  onResize() { this.isMobile = window.innerWidth <= 768; }

  canDropItem = (drag: CdkDrag): boolean => {
    return !!(drag.data && !('children' in drag.data));
  };

  private getSiblings(): StorageLocation[] {
    const parent = this.facade.locations.flatLocations.find(l => l.children?.some(c => c.id === this.location.id));
    return parent && parent.children ? parent.children : this.facade.locations.locations;
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
    // Закрываем меню итемов
    if (this.openItemMenuId && !this.el.nativeElement.contains(event.target)) {
      this.openItemMenuId = null;
    }

    // Автоматически закрываем открытые <details> при клике мимо
    const openDetails = this.el.nativeElement.querySelectorAll('details[open]');
    openDetails.forEach((details: HTMLDetailsElement) => {
      if (!details.contains(event.target as Node)) {
        details.removeAttribute('open');
      }
    });
  }

  onItemDrop(event: CdkDragDrop<Item[]>) {
    this.itemDropped.emit({ event, loc: this.location });
  }

  onAddLocation(event: Event) {
    event.stopPropagation();
    if (this.isMaxLevelReached) return;

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

  trackById(_: number, item: any): string {
    return item.id;
  }
}