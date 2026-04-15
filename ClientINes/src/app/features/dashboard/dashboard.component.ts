import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';

import { DashboardFacade } from './dashboard.facade';
import { DashboardModalService } from './dashboard.modal.service';
import { LocationCardComponent } from '../../shared/components/location-card/location-card.component';
import { LocationRibbonComponent } from '../../shared/components/location-ribbon/location-ribbon.component';
import { RIBBON_CONFIG, BOARD_CONFIG } from '../../shared/constants/ui.constants';
import { StorageLocation } from '../../models/entities/storage-location.entity';
import { Item } from '../../models/entities/item.entity';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, DragDropModule, LocationCardComponent, LocationRibbonComponent, TranslateModule],
  providers: [DashboardFacade],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  public facade = inject(DashboardFacade); 
  public modal = inject(DashboardModalService);
  private sub = new Subscription();

  currentPageBoard = 0;
  currentPageRibbon = 0;

  get ribbonPageSize(): number {
    return window.innerWidth <= RIBBON_CONFIG.BREAKPOINT_MOBILE 
      ? RIBBON_CONFIG.PAGE_SIZE_MOBILE 
      : RIBBON_CONFIG.PAGE_SIZE_DESKTOP;
  }

  get pagedBoardLocations(): StorageLocation[] {
    const start = this.currentPageBoard * BOARD_CONFIG.PAGE_SIZE;
    return this.facade.locations.slice(start, start + BOARD_CONFIG.PAGE_SIZE);
  }

  get activeBoardIds(): string[] {
    return this.pagedBoardLocations.map(l => l.id);
  }

  get totalBoardPages(): number { 
    return Math.ceil(this.facade.locations.length / BOARD_CONFIG.PAGE_SIZE); 
  }

  ngOnInit() { 
    this.loadData(); 
    this.sub.add(
      this.modal.refreshData$.subscribe(() => this.loadData())
    );
  }

  ngOnDestroy() { this.sub.unsubscribe(); }

  loadData() { this.facade.loadData().subscribe(); }

  changeBoardPage(delta: number) {
    this.currentPageBoard += delta;
    this.syncRibbonWithBoard();
  }

  private syncRibbonWithBoard() {
    const firstVisibleIndex = this.currentPageBoard * BOARD_CONFIG.PAGE_SIZE;
    this.currentPageRibbon = Math.floor(firstVisibleIndex / this.ribbonPageSize);
  }

  jumpToLocation(locId: string) {
    const index = this.facade.locations.findIndex(l => l.id === locId);
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

  onEditItem(item: Item) {
    this.modal.openItemForm(item).subscribe();
    }

    onCreateItem(locId?: string) {
    this.modal.openItemForm(null, locId).subscribe();
    }

  // --- ЛОГИКА МОДАЛОК ---

  onRename(loc: StorageLocation) { 
    loc.showMenu = false;
    this.modal.openConfirm({
      mode: 'input', title: 'COMMON.RENAME', message: '', confirmText: 'COMMON.SAVE', name: loc.name
    }).subscribe(newName => {
      if (newName) this.facade.renameLocation(loc.id, newName).subscribe();
    });
  }

  onDeleteLocation(loc: StorageLocation) { 
    loc.showMenu = false;
    this.modal.openConfirm({
      mode: 'delete', title: 'COMMON.DELETE', message: 'LOCATION_CARD.MODAL.YOU_SURE_MSG'
    }).subscribe(() => {
      this.facade.deleteLocation(loc.id).subscribe(() => {
        if (this.currentPageBoard > 0 && this.pagedBoardLocations.length === 0) this.currentPageBoard--;
        this.syncRibbonWithBoard();
      });
    });
  }

  onDeleteItem(item: Item) { 
    this.modal.openConfirm({
      mode: 'delete', title: 'COMMON.DELETE', message: 'ITEM_CARD.MODAL.YOU_SURE_MSG'
    }).subscribe(() => {
      this.facade.deleteItem(item.id).subscribe(() => this.loadData());
    });
  }

  onSellRequest(item: Item) { 
    this.modal.openSell(item).subscribe(dto => {
      if (dto) this.facade.sellItem(dto).subscribe();
    });
  }

  onLendRequest(item: Item) { 
    this.modal.openLend(item).subscribe(dto => {
      if (dto) this.facade.lendItem(dto).subscribe(() => this.loadData());
    });
  }

  // --- DRAG AND DROP & MOVEMENT ---
  
  onItemMoveManual(data: {item: Item, targetLocationId: string}) {
    const targetLoc = this.facade.flatLocations.find(l => l.id === data.targetLocationId);
    this.modal.openConfirm({
      mode: 'confirm', title: 'ITEM_CARD.MODAL.MOVE_TITLE', message: targetLoc?.name || '...', confirmText: 'COMMON.YES'
    }).subscribe(() => {
      this.facade.moveItemLocally(data.item, data.targetLocationId);
      this.facade.moveItemApi(data.item.id, data.targetLocationId).subscribe({
        next: () => this.jumpToLocation(data.targetLocationId),
        error: () => this.loadData()
      });
    });
  }

  onRibbonReorder(event: CdkDragDrop<StorageLocation[]>) {
    const offset = this.currentPageRibbon * this.ribbonPageSize;
    moveItemInArray(this.facade.locations, event.previousIndex + offset, event.currentIndex + offset);
    this.facade.reorderLocations(this.facade.locations.map(l => l.id)).subscribe();
  }

  onLocationMove(event: { loc: StorageLocation, targetId: string }) {
    const targetId = event.targetId === 'root' ? null : event.targetId;
    this.facade.moveLocationLocally(event.loc.id, targetId);
    this.facade.moveLocation(event.loc.id, event.targetId).subscribe({ error: () => this.loadData() });
  }

  onItemDropped(data: {event: CdkDragDrop<Item[]>, loc: StorageLocation}) {
    const { event, loc } = data;
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }
    const item = event.previousContainer.data[event.previousIndex];
    this.facade.moveItemLocally(item, loc.id);
    this.facade.moveItemApi(item.id, loc.id).subscribe({ error: () => this.loadData() });
  }

  isChildOf = (targetId: string, sourceLoc: StorageLocation) => this.facade.isChildOf(targetId, sourceLoc);
  trackById = (index: number, item: any) => item.id;
}