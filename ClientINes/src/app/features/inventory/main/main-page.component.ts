import { Component, OnInit, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray} from '@angular/cdk/drag-drop';
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

import { LendingService } from '../../../shared/services/lending.service';
import { LendItemModalComponent } from '../../../shared/components/modal/lend-modal/lend-item-modal.component';
import { LendItemDto } from '../../../models/dtos/lending.dto';
import { RIBBON_CONFIG, BOARD_CONFIG } from '../../../shared/constants/ui.constants';

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
  currentPageRibbon = 0;

  get ribbonPageSize(): number {
    return window.innerWidth <= RIBBON_CONFIG.BREAKPOINT_MOBILE 
      ? RIBBON_CONFIG.PAGE_SIZE_MOBILE 
      : RIBBON_CONFIG.PAGE_SIZE_DESKTOP;
  }

  get pagedBoardLocations(): StorageLocation[] {
    const start = this.currentPageBoard * BOARD_CONFIG.PAGE_SIZE;
    return this.locations.slice(start, start + BOARD_CONFIG.PAGE_SIZE);
  }

  get activeBoardIds(): string[] {
    return this.pagedBoardLocations.map(l => l.id);
  }

  get totalBoardPages(): number { 
    return Math.ceil(this.locations.length / BOARD_CONFIG.PAGE_SIZE); 
  }

  ngOnInit() { this.loadData(); }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    this.locations.forEach(loc => this.closeMenuRecursive(loc, event));
  }

  // --- ЛОГИКА СИНХРОНИЗАЦИИ ---

  changeBoardPage(delta: number) {
    this.currentPageBoard += delta;
    this.syncRibbonWithBoard();
  }

  private syncRibbonWithBoard() {
    const firstVisibleIndex = this.currentPageBoard * BOARD_CONFIG.PAGE_SIZE;
    this.currentPageRibbon = Math.floor(firstVisibleIndex / this.ribbonPageSize);
  }

  jumpToLocation(locId: string) {
    const index = this.locations.findIndex(l => l.id === locId);
    if (index !== -1) {
      this.currentPageBoard = Math.floor(index / BOARD_CONFIG.PAGE_SIZE);
      this.syncRibbonWithBoard();
    }
  }

  onRibbonPageChange(newPage: number) {
    this.currentPageRibbon = newPage;
    const firstItemIndex = newPage * this.ribbonPageSize;
    this.currentPageBoard = Math.floor(firstItemIndex / BOARD_CONFIG.PAGE_SIZE);
  }

  // --- ДАННЫЕ И СОСТОЯНИЕ ---

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

  // --- ЛОКАЦИИ И ПРЕДМЕТЫ (Методы оставлены без изменений для краткости) ---
  
  onRename(loc: StorageLocation) { this.selectedLocation = loc; this.activeModal = 'renameLoc'; loc.showMenu = false; }
  onDeleteLocation(loc: StorageLocation) { this.selectedLocation = loc; this.activeModal = 'deleteLoc'; loc.showMenu = false; }
  onDeleteItem(item: Item) { this.selectedItem = item; this.activeModal = 'deleteItem'; }
  onSellRequest(item: Item) { this.selectedItem = item; this.activeModal = 'sell'; }
  onLendRequest(item: Item) { this.selectedItem = item; this.activeModal = 'lend'; }

  confirmDeleteLocation() {
    if (!this.selectedLocation) return;
    this.locationService.delete(this.selectedLocation.id).subscribe({
      next: () => {
        this.removeLocationFromTree(this.locations, this.selectedLocation!.id);
        this.locations = [...this.locations];
        this.refreshState();
        if (this.currentPageBoard > 0 && this.pagedBoardLocations.length === 0) this.currentPageBoard--;
        this.syncRibbonWithBoard();
        this.closeModals();
      }
    });
  }

  confirmDeleteItem() {
    if (!this.selectedItem) return;
    this.itemService.delete(this.selectedItem.id).subscribe(() => { this.loadData(); this.closeModals(); });
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

  // --- ОБЩЕЕ ---

  closeModals() { this.activeModal = null; this.selectedItem = null; this.selectedLocation = null; this.pendingLocationId = null; }

  confirmNavigation() {
    if (this.pendingLocationId) this.jumpToLocation(this.pendingLocationId);
    this.closeModals();
  }

  get modalConfig() {
    const type = this.activeModal;
    if (!type || type === 'sell' || type === 'lend') return null;
    const configs: Record<string, any> = {
      renameLoc: { mode: 'input', title: 'COMMON.RENAME', message: '', confirmText: 'COMMON.SAVE', cancelText: 'COMMON.CANCEL', name: this.selectedLocation?.name },
      deleteLoc: { mode: 'delete', title: 'COMMON.DELETE', message: 'LOCATION_CARD.MODAL.YOU_SURE_MSG' },
      deleteItem: { mode: 'delete', title: 'COMMON.DELETE', message: 'ITEM_CARD.MODAL.YOU_SURE_MSG' },
      moveConfirm: { mode: 'input', title: 'COMMON.CONFIRM_MOVE', message: 'LOCATIONS.MODAL.M_MOVE_SUCCESS', name: 'skip', confirmText: 'COMMON.GO_TO', cancelText: 'COMMON.STAY_HERE' }
    };
    return configs[type];
  }

  handleModalConfirm(value: string) {
    switch (this.activeModal) {
      case 'renameLoc': this.confirmRename(value); break;
      case 'deleteLoc': this.confirmDeleteLocation(); break;
      case 'deleteItem': this.confirmDeleteItem(); break;
      case 'moveConfirm': this.confirmNavigation(); break;
    }
  }

  onRibbonReorder(event: CdkDragDrop<StorageLocation[]>) {
    const offset = this.currentPageRibbon * this.ribbonPageSize;
    moveItemInArray(this.locations, event.previousIndex + offset, event.currentIndex + offset);
    this.locationService.reorder({ parentId: null, orderedIds: this.locations.map(l => l.id) }).subscribe();
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

  confirmRename(newName: string) {
    if (!this.selectedLocation) return;
    this.locationService.rename(this.selectedLocation.id, newName).subscribe(() => {
      this.selectedLocation!.name = newName;
      this.closeModals();
    });
  }

  onSellConfirmed(dto: SellItemRequestDto) {
    this.salesService.sellItem(dto).subscribe(() => { this.closeModals(); this.router.navigate(['/sales']); });
  }

  onLendConfirmed(dto: LendItemDto) {
    this.lendingService.lendItem(dto).subscribe({ next: () => { this.loadData(); this.closeModals(); } });
  }
}