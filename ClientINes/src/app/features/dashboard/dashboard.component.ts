import { Component, OnInit, OnDestroy, inject, HostListener } from '@angular/core';
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
import { CdkDrag } from '@angular/cdk/drag-drop';

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

  isMobile = window.innerWidth <= 768;

  currentPageBoard = 0;
  currentPageRibbon = 0;

  isDraggingLoc = false;

  onLocationDragStart() {
    this.isDraggingLoc = true;
  }

  onLocationDragEnd() {
    this.isDraggingLoc = false;
  }

  onLocationMoveUp(loc: StorageLocation) { this.facade.moveLocationUpDown(loc.id, 'up'); }
  onLocationMoveDown(loc: StorageLocation) { this.facade.moveLocationUpDown(loc.id, 'down'); }

  get ribbonPageSize(): number { return window.innerWidth <= RIBBON_CONFIG.BREAKPOINT_MOBILE ? RIBBON_CONFIG.PAGE_SIZE_MOBILE : RIBBON_CONFIG.PAGE_SIZE_DESKTOP; }
  get pagedBoardLocations(): StorageLocation[] { const start = this.currentPageBoard * BOARD_CONFIG.PAGE_SIZE; return this.facade.locations.slice(start, start + BOARD_CONFIG.PAGE_SIZE); }
  get activeBoardIds(): string[] { return this.pagedBoardLocations.map(l => l.id); }
  get totalBoardPages(): number { return Math.ceil(this.facade.locations.length / BOARD_CONFIG.PAGE_SIZE); }
  
  get visibleConnectedLists(): string[] { return this.facade.flattenLocations(this.pagedBoardLocations).map(l => l.id); }
  get visibleConnectedLocationLists(): string[] { 
    return this.facade.flattenLocations(this.pagedBoardLocations).map(l => 'list-loc-' + l.id); 
  }

  ngOnInit() { this.loadData(); this.sub.add(this.modal.refreshData$.subscribe(() => this.loadData())); }
  ngOnDestroy() { this.sub.unsubscribe(); }
  loadData() { this.facade.loadData().subscribe(); }

  changeBoardPage(delta: number) { this.currentPageBoard += delta; this.syncRibbonWithBoard(); }
  private syncRibbonWithBoard() { const firstVisibleIndex = this.currentPageBoard * BOARD_CONFIG.PAGE_SIZE; this.currentPageRibbon = Math.floor(firstVisibleIndex / this.ribbonPageSize); }
  jumpToLocation(locId: string) { const index = this.facade.locations.findIndex(l => l.id === locId); if (index !== -1) { this.currentPageBoard = Math.floor(index / BOARD_CONFIG.PAGE_SIZE); this.syncRibbonWithBoard(); } }
  onRibbonPageChange(newPage: number) { this.currentPageRibbon = newPage; const firstItemIndex = newPage * this.ribbonPageSize; this.currentPageBoard = Math.floor(firstItemIndex / BOARD_CONFIG.PAGE_SIZE); }

  onEditItem(item: Item) { this.modal.openItemForm(item).subscribe(); }
  onCreateItem(locId?: string) { this.modal.openItemForm(null, locId).subscribe(); }
  onRename(loc: StorageLocation) { loc.showMenu = false; this.modal.openConfirm({ mode: 'input', title: 'COMMON.RENAME', message: '', confirmText: 'COMMON.SAVE', name: loc.name }).subscribe(newName => { if (newName) this.facade.renameLocation(loc.id, newName).subscribe(); }); }
  onDeleteLocation(loc: StorageLocation) { loc.showMenu = false; this.modal.openConfirm({ mode: 'delete', title: 'COMMON.DELETE', message: 'LOCATION_CARD.MODAL.YOU_SURE_MSG' }).subscribe(() => { this.facade.deleteLocation(loc.id).subscribe(() => { if (this.currentPageBoard > 0 && this.pagedBoardLocations.length === 0) this.currentPageBoard--; this.syncRibbonWithBoard(); }); }); }
  onDeleteItem(item: Item) { this.modal.openConfirm({ mode: 'delete', title: 'COMMON.DELETE', message: 'ITEM_CARD.MODAL.YOU_SURE_MSG' }).subscribe(() => { this.facade.deleteItem(item.id).subscribe(() => this.loadData()); }); }
  onSellRequest(item: Item) { this.modal.openSell(item).subscribe(dto => { if (dto) this.facade.sellItem(dto).subscribe(); }); }
  onLendRequest(item: Item) { this.modal.openLend(item).subscribe(dto => { if (dto) this.facade.lendItem(dto).subscribe(() => this.loadData()); }); }

  onItemMoveManual(data: {item: Item, targetLocationId: string}) { const targetLoc = this.facade.flatLocations.find(l => l.id === data.targetLocationId); this.modal.openConfirm({ mode: 'confirm', title: 'ITEM_CARD.MODAL.MOVE_TITLE', message: targetLoc?.name || '...', confirmText: 'COMMON.YES' }).subscribe(() => { this.facade.moveItemLocally(data.item, data.targetLocationId); this.facade.moveItemApi(data.item.id, data.targetLocationId).subscribe({ next: () => this.jumpToLocation(data.targetLocationId), error: () => this.loadData() }); }); }
  onRibbonReorder(event: CdkDragDrop<StorageLocation[]>) { const offset = this.currentPageRibbon * this.ribbonPageSize; moveItemInArray(this.facade.locations, event.previousIndex + offset, event.currentIndex + offset); this.facade.reorderLocations(this.facade.locations.map(l => l.id)).subscribe(); }

  onLocationMove(event: { loc: StorageLocation, targetId: string }) {
    const normalizedTargetId = event.targetId === 'root' ? null : event.targetId;
    this.facade.moveLocationApi(event.loc.id, normalizedTargetId).subscribe({
      next: () => this.loadData(),
      error: (err) => { 
        if (err === 'TOO_DEEP') alert('ERRORS.MAX_NESTING_REACHED');
        this.loadData(); 
      }
    });
  }

  canDropRootLocation = (drag: CdkDrag): boolean => {
    const data = drag.data;
    if (!data || !('children' in data)) return false;

    return this.facade.canMoveLocation(data.id, null);
  };

  onLocationDropped(data: {event: CdkDragDrop<StorageLocation[]>, targetId: string | null}) {
    const { event, targetId } = data;
    const normalizedTargetId = targetId === 'root' ? null : targetId;
    const loc = event.previousContainer.data[event.previousIndex];

    // Если дропнули в ту же папку - игнорим, для сортировки есть кнопки
    if (event.previousContainer === event.container) return;

    // Вырезаем локацию из старого списка
    if (event.previousContainer.id === 'root-loc-list') {
      const offset = this.currentPageBoard * BOARD_CONFIG.PAGE_SIZE;
      this.facade.locations.splice(event.previousIndex + offset, 1);
      this.facade.locations = [...this.facade.locations];
    } else {
      event.previousContainer.data.splice(event.previousIndex, 1);
    }

    // Просто пушим в конец новой папки (в корень нельзя по ТЗ)
    event.container.data.push(loc);
    this.facade.refreshState();

    // API: перемещаем и сохраняем новый порядок
    this.facade.moveLocationApi(loc.id, normalizedTargetId).subscribe({
      next: () => {
        const orderedIds = event.container.data.map(l => l.id);
        this.facade.reorderLocations(orderedIds, normalizedTargetId).subscribe({
          error: () => this.loadData()
        });
      },
      error: (err) => {
        if (err === 'TOO_DEEP') alert('ERRORS.MAX_NESTING_REACHED');
        this.loadData();
      }
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
    this.facade.moveItemApi(item.id, loc.id).subscribe({ error: () => this.loadData() });
  }

  isChildOf = (targetId: string, sourceLoc: StorageLocation) => this.facade.isChildOf(targetId, sourceLoc);
  trackById = (index: number, item: any) => item.id;
}