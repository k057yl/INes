import { Component, OnInit, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';

import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { Item } from '../../../models/entities/item.entity';
import { LocationCardComponent } from '../../../shared/components/location-card/location-card.component';
import { LocationRibbonComponent } from '../../../shared/components/location-ribbon/location-ribbon.component';
import { SalesService } from '../../../shared/services/sales.service';
import { LocationService } from '../../../shared/services/location.service';
import { ItemService } from '../../../shared/services/item.service';
import { SellModalComponent } from '../../../shared/components/sell-modal/sell-modal.component';
import { InestModalComponent } from '../../../shared/components/modal/shared-modal/inest-modal.component';
import { SellItemRequestDto } from '../../../models/dtos/sale.dto';
import { ItemStatus } from '../../../models/enums/item-status.enum';

import { LendingService } from '../../../shared/services/lending.service';
import { LendItemModalComponent } from '../../../shared/components/modal/lend-modal/lend-item-modal.component';
import { LendItemDto } from '../../../models/dtos/lending.dto';

type ModalType = 'deleteItem' | 'deleteLoc' | 'renameLoc' | 'moveConfirm' | 'sell' | 'lend' | null;

@Component({
  selector: 'app-main-page',
  standalone: true,
  imports: [
    CommonModule, RouterModule, DragDropModule, 
    LocationCardComponent, LocationRibbonComponent, 
    TranslateModule, SellModalComponent, InestModalComponent, LendItemModalComponent
  ],
  templateUrl: './main-page.component.html',
  styleUrl: './main-page.component.scss'
})
export class MainPageComponent implements OnInit {
  private locationService = inject(LocationService);
  private itemService = inject(ItemService);
  private salesService = inject(SalesService);
  private router = inject(Router);
  private lendingService = inject(LendingService);

  locations: StorageLocation[] = [];
  flatLocations: StorageLocation[] = [];
  connectedLists: string[] = [];
  isLoading = true;
  
  // ЦЕНТРАЛЬНЫЙ СТЭЙТ МОДАЛОК
  activeModal: ModalType = null;
  selectedItem: Item | null = null;
  selectedLocation: StorageLocation | null = null;
  pendingLocationId: string | null = null;

  currentPageBoard = 0;
  readonly pageSizeBoard = 3;
  currentPageRibbon = 0;
  readonly pageSizeRibbon = 15;

  get pagedBoardLocations(): StorageLocation[] {
    const start = this.currentPageBoard * this.pageSizeBoard;
    return this.locations.slice(start, start + this.pageSizeBoard);
  }

  get activeBoardIds(): string[] {
    return this.pagedBoardLocations.map(l => l.id);
  }

  get totalBoardPages(): number { 
    return Math.ceil(this.locations.length / this.pageSizeBoard); 
  }

  ngOnInit() { this.loadData(); }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    this.locations.forEach(loc => this.closeMenuRecursive(loc, event));
  }

  loadData() {
    this.isLoading = true;
    this.locationService.getTree().subscribe({
      next: (data) => {
        this.locations = data;
        this.refreshState();
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  refreshState() {
    this.flatLocations = this.flattenLocations(this.locations);
    this.connectedLists = this.flatLocations.map(l => l.id);
  }

  private flattenLocations(locs: StorageLocation[]): StorageLocation[] {
    return locs.reduce<StorageLocation[]>((acc, l) => {
      acc.push(l);
      if (l.children?.length) acc.push(...this.flattenLocations(l.children));
      return acc;
    }, []);
  }

  // --- ЛОКАЦИИ ---

  onRename(loc: StorageLocation) {
    this.selectedLocation = loc;
    this.activeModal = 'renameLoc';
    loc.showMenu = false;
  }

  confirmRename(newName: string) {
    if (!this.selectedLocation) return;
    this.locationService.rename(this.selectedLocation.id, newName).subscribe(() => {
      this.selectedLocation!.name = newName;
      this.closeModals();
    });
  }

  onDeleteLocation(loc: StorageLocation) {
    this.selectedLocation = loc;
    this.activeModal = 'deleteLoc';
    loc.showMenu = false;
  }

  confirmDeleteLocation() {
    if (!this.selectedLocation) return;
    this.locationService.delete(this.selectedLocation.id).subscribe(() => {
      this.removeLocationFromTree(this.locations, this.selectedLocation!.id);
      this.locations = [...this.locations];
      this.refreshState();
      if (this.currentPageBoard > 0 && this.pagedBoardLocations.length === 0) this.currentPageBoard--;
      this.closeModals();
    });
  }

  // --- ПРЕДМЕТЫ ---

  onDeleteItem(item: Item) {
    this.selectedItem = item;
    this.activeModal = 'deleteItem';
  }

  confirmDeleteItem() {
    if (!this.selectedItem) return;
    this.itemService.delete(this.selectedItem.id).subscribe({
      next: () => {
        this.loadData(); 
        this.closeModals();
      },
      error: (err) => { console.error('F*ck:', err); this.closeModals(); }
    });
  }

  onItemMoveManual(data: {item: Item, targetLocationId: string}) {
    const { item, targetLocationId } = data;
    this.itemService.move(item.id, targetLocationId).subscribe({
      next: () => {
        this.loadData();
        if (!this.activeBoardIds.includes(targetLocationId)) {
          this.pendingLocationId = targetLocationId;
          this.activeModal = 'moveConfirm';
        }
      }
    });
  }

  // --- ПРОДАЖИ & ЛЕНДИНГ ---

  onSellRequest(item: Item) { 
    this.selectedItem = item; 
    this.activeModal = 'sell';
  }

  onLendRequest(item: Item) {
    this.selectedItem = item;
    this.activeModal = 'lend';
  }

  onSellConfirmed(dto: SellItemRequestDto) {
    this.salesService.sellItem(dto).subscribe(() => { 
      this.closeModals();
      this.router.navigate(['/sales']); 
    });
  }

  onLendConfirmed(dto: LendItemDto) {
    this.lendingService.lendItem(dto).subscribe({
      next: () => { this.loadData(); this.closeModals(); },
      error: () => this.closeModals()
    });
  }

  // --- ОБЩЕЕ ---

  closeModals() {
    this.activeModal = null;
    this.selectedItem = null;
    this.selectedLocation = null;
    this.pendingLocationId = null;
  }

  confirmNavigation() {
    if (this.pendingLocationId) this.jumpToLocation(this.pendingLocationId);
    this.closeModals();
  }

  // Вспомогательные методы
  onRibbonReorder(event: CdkDragDrop<StorageLocation[]>) {
    const pageSize = window.innerWidth <= 768 ? 9 : 15;
    const offset = this.currentPageRibbon * pageSize;
    moveItemInArray(this.locations, event.previousIndex + offset, event.currentIndex + offset);
    this.locationService.reorder({ parentId: null, orderedIds: this.locations.map(l => l.id) }).subscribe();
  }

  jumpToLocation(locId: string) {
    const index = this.locations.findIndex(l => l.id === locId);
    if (index !== -1) this.currentPageBoard = Math.floor(index / this.pageSizeBoard);
  }

  onLocationMove(event: { loc: StorageLocation, targetId: string }) {
    const newParentId = event.targetId === 'root' ? null : event.targetId;
    this.locationService.move(event.loc.id, newParentId).subscribe(() => this.loadData());
  }

  onItemDropped(data: {event: CdkDragDrop<Item[]>, loc: StorageLocation}) {
    const { event, loc } = data;
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }
    const item = event.previousContainer.data[event.previousIndex];
    this.itemService.move(item.id, loc.id).subscribe(() => {
        this.loadData();
        if (loc.isLendingLocation) setTimeout(() => this.onLendRequest(item), 200);
    });
  }

  isChildOf(targetId: string, sourceLoc: StorageLocation): boolean {
    return sourceLoc.children?.some(c => c.id === targetId || this.isChildOf(targetId, c)) || false;
  }

  private removeLocationFromTree(tree: StorageLocation[], id: string): boolean {
    for (let i = 0; i < tree.length; i++) {
      if (tree[i].id === id) { tree.splice(i, 1); return true; }
      if (tree[i].children?.length && this.removeLocationFromTree(tree[i].children!, id)) return true;
    }
    return false;
  }

  private closeMenuRecursive(loc: StorageLocation, event: MouseEvent) {
    if (loc.showMenu) {
      const target = event.target as HTMLElement;
      if (!target.closest('.menu-dropdown') && !target.closest('.inest-action-btn')) loc.showMenu = false;
    }
    loc.children?.forEach(c => this.closeMenuRecursive(c, event));
  }

  trackById = (index: number, item: any) => item.id;
}