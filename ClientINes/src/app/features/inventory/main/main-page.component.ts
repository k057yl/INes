import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray} from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';

import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { Item } from '../../../models/entities/item.entity';
import { LocationCardComponent } from '../../../shared/components/location-card/location-card.component';
import { LocationRibbonComponent } from '../../../shared/components/location-ribbon/location-ribbon.component';
import { SellModalComponent } from '../../../shared/components/sell-modal/sell-modal.component';
import { InestModalComponent } from '../../../shared/components/modal/shared-modal/inest-modal.component';
import { LendItemModalComponent } from '../../../shared/components/modal/lend-modal/lend-item-modal.component';
import { RIBBON_CONFIG, BOARD_CONFIG } from '../../../shared/constants/ui.constants';

import { MainPageFacade } from './main-page.facade';
import { MainPageModalService } from './main-page.modal.service';

@Component({
  selector: 'app-main-page',
  standalone: true,
  imports: [
    CommonModule, RouterModule, DragDropModule, 
    LocationCardComponent, LocationRibbonComponent, 
    TranslateModule, SellModalComponent, InestModalComponent, LendItemModalComponent
  ],
  providers: [MainPageFacade, MainPageModalService],
  templateUrl: './main-page.component.html',
  styleUrl: './main-page.component.scss'
})
export class MainPageComponent implements OnInit {
  public facade = inject(MainPageFacade); 
  public modal = inject(MainPageModalService);

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

  ngOnInit() { this.loadData(); }

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

  loadData() {
    this.facade.loadData().subscribe();
  }

  // --- ЧИСТАЯ ЛОГИКА МОДАЛОК ---

  onRename(loc: StorageLocation) { 
    loc.showMenu = false;
    this.modal.openRename(loc).subscribe(newName => {
      if (newName) this.facade.renameLocation(loc.id, newName).subscribe();
    });
  }

  onDeleteLocation(loc: StorageLocation) { 
    loc.showMenu = false;
    this.modal.openDeleteLocation(loc).subscribe(() => {
      this.facade.deleteLocation(loc.id).subscribe(() => {
        if (this.currentPageBoard > 0 && this.pagedBoardLocations.length === 0) this.currentPageBoard--;
        this.syncRibbonWithBoard();
      });
    });
  }

  onDeleteItem(item: Item) { 
    this.modal.openDeleteItem(item).subscribe(() => {
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

  onItemMoveManual(data: {item: Item, targetLocationId: string}) {
    this.modal.openMoveConfirm().subscribe(() => {
      this.facade.moveItemLocally(data.item, data.targetLocationId);
      this.facade.moveItemApi(data.item.id, data.targetLocationId).subscribe({
        next: () => this.jumpToLocation(data.targetLocationId),
        error: () => this.loadData()
      });
    });
  }

  // --- DRAG AND DROP ---
  onRibbonReorder(event: CdkDragDrop<StorageLocation[]>) {
    const offset = this.currentPageRibbon * this.ribbonPageSize;
    moveItemInArray(this.facade.locations, event.previousIndex + offset, event.currentIndex + offset);
    this.facade.reorderLocations(this.facade.locations.map(l => l.id)).subscribe();
  }

  onLocationMove(event: { loc: StorageLocation, targetId: string }) {
    const targetId = event.targetId === 'root' ? null : event.targetId;

    // Оптимистично двигаем в UI, чтобы не было мигания
    this.facade.moveLocationLocally(event.loc.id, targetId);

    this.facade.moveLocation(event.loc.id, event.targetId).subscribe({
      error: () => this.loadData() // Откат, если сервер помер
    });
  }

  onItemDropped(data: {event: CdkDragDrop<Item[]>, loc: StorageLocation}) {
    const { event, loc } = data;
    
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }

    const item = event.previousContainer.data[event.previousIndex];
    this.facade.moveItemLocally(item, loc.id);

    this.facade.moveItemApi(item.id, loc.id).subscribe({
      error: () => this.loadData()
    });

    if (loc.isLendingLocation) {
      setTimeout(() => this.onLendRequest(item), 200);
    }
  }

  isChildOf = (targetId: string, sourceLoc: StorageLocation): boolean => {
    return this.facade.isChildOf(targetId, sourceLoc);
  }

  trackById = (index: number, item: any) => item.id;
}