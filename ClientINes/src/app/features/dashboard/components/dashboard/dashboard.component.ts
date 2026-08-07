import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray, CdkDrag } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';

import { DashboardFacade } from './dashboard.facade';
import { DashboardModalService } from './dashboard.modal.service';
import { DashboardTreeService } from '../../services/dashboard-tree.service';
import { DashboardLocationService } from '../../services/dashboard-location.service';
import { DashboardItemService } from '../../services/dashboard-item.service';
import { DashboardNavigationService } from '../../services/dashboard-navigation.service';
import { DashboardActionExecutor } from '../../services/dashboard-action-executor.service';
import { DashboardStatsComponent } from '../dashboard-stats/dashboard-stats.component';
import { StatsListModalComponent } from '../stats-list-modal/stats-list-modal.component';

import { LocationCardComponent } from '../../../location/components/location-card/location-card.component';
import { LocationRibbonComponent } from '../location-ribbon/location-ribbon.component';
import { StorageLocation } from '../../../location/contracts/storage-location';
import { Item } from '../../../item/contracts/item';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, DragDropModule, LocationCardComponent, LocationRibbonComponent, DashboardStatsComponent, StatsListModalComponent, TranslateModule],
  providers: [
    DashboardTreeService,
    DashboardLocationService,
    DashboardItemService,
    DashboardNavigationService,
    DashboardActionExecutor,
    DashboardFacade
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  public facade = inject(DashboardFacade);
  public modal = inject(DashboardModalService);

  private sub = new Subscription();
  isMobile = window.innerWidth <= 768;

  ngOnInit() {
    this.loadData();
    this.sub.add(this.modal.refreshData$.subscribe(() => this.loadData()));
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  loadData() {
    this.facade.loadData().subscribe();
  }

  // --- Навигация и пагинация ---
  get pagedBoardLocations(): StorageLocation[] {
    return this.facade.nav.getBoardPageLocations(this.facade.locations.locations);
  }

  get activeBoardIds(): string[] {
    return this.pagedBoardLocations.map(l => l.id);
  }

  get totalBoardPages(): number {
    return this.facade.nav.getTotalBoardPages(this.facade.locations.locations.length);
  }

  changeBoardPage(delta: number) {
    this.facade.nav.changeBoardPage(delta, this.facade.locations.locations.length);
  }

  jumpToLocation(locId: string) {
    this.facade.nav.jumpToLocation(locId, this.facade.locations.locations);
  }

  onRibbonPageChange(newPage: number) {
    this.facade.nav.onRibbonPageChange(newPage);
  }

  // --- Действия с локациями ---
  onRename(loc: StorageLocation) {
    loc.showMenu = false;
    this.modal.openConfirm({ mode: 'input', title: 'COMMON.RENAME', message: '', confirmText: 'COMMON.SAVE', name: loc.name })
      .subscribe(newName => {
        if (newName) {
          this.facade.executor.run(this.facade.locations.rename(loc.id, newName), 'LOCATIONS.SUCCESS.RENAME');
        }
      });
  }

  onDeleteLocation(loc: StorageLocation) {
    loc.showMenu = false;
    this.modal.openConfirm({ mode: 'delete', title: 'COMMON.DELETE', message: 'LOCATION_CARD.MODAL.YOU_SURE_MSG' })
      .subscribe(confirmed => {
        if (confirmed) {
          this.facade.executor.run(
            this.facade.locations.delete(loc.id),
            'LOCATIONS.SUCCESS.DELETE',
            () => {
              this.facade.nav.adjustPageAfterDelete(this.pagedBoardLocations.length);
              this.loadData();
            }
          );
        }
      });
  }

  onLocationMoveUp(loc: StorageLocation) {
    this.facade.executor.run(this.facade.locations.moveUpDown(loc.id, 'up'), null);
  }

  onLocationMoveDown(loc: StorageLocation) {
    this.facade.executor.run(this.facade.locations.moveUpDown(loc.id, 'down'), null);
  }

  // --- Действия с предметами ---
  onEditItem(item: Item) { this.modal.openItemForm(item).subscribe(); }
  onCreateItem(locId?: string) { this.modal.openItemForm(null, locId).subscribe(); }

  onDeleteItem(item: Item) {
    this.modal.openConfirm({ mode: 'delete', title: 'COMMON.DELETE', message: 'ITEM_CARD.MODAL.YOU_SURE_MSG' })
      .subscribe(confirmed => {
        if (confirmed) {
          this.facade.executor.run(this.facade.items.delete(item.id), 'ITEMS.SUCCESS.DELETE', () => this.loadData());
        }
      });
  }

  onSellRequest(item: Item) {
    this.modal.openSell(item).subscribe(dto => {
      if (dto) {
        this.facade.executor.run(this.facade.items.sell(dto), 'SALES.SUCCESS.SELL');
      }
    });
  }

  onLendRequest(item: Item) {
    this.modal.openLend(item).subscribe(dto => {
      if (dto) {
        this.facade.executor.run(this.facade.items.lend(dto), 'LENDING.SUCCESS.LEND', () => this.loadData());
      }
    });
  }

  onReturnRequest(item: Item) {
    this.modal.openConfirm({ mode: 'confirm', title: 'COMMON.RETURN', message: 'LENDING_MODAL.MODAL.RETURN_MSG', confirmText: 'COMMON.YES' })
      .subscribe(confirmed => {
        if (confirmed) {
          this.facade.executor.run(this.facade.items.returnItem(item.id), 'LENDING_MODAL.SUCCESS_TOASTER', () => this.loadData());
        }
      });
  }

  onItemStatusChange(event: { item: Item, newStatus: number }) {
    this.facade.executor.run(this.facade.items.changeStatus(event.item.id, event.newStatus), 'COMMON.CHANGE_STATUS', () => this.loadData());
  }

  onItemMoveManual(data: { item: Item, targetLocationId: string }) {
    const targetLoc = this.facade.locations.flatLocations.find(l => l.id === data.targetLocationId);
    this.modal.openConfirm({ mode: 'confirm', title: 'ITEM_CARD.MODAL.MOVE_TITLE', message: targetLoc?.name || '...', confirmText: 'COMMON.YES' })
      .subscribe(confirmed => {
        if (confirmed) {
          this.facade.items.moveLocally(data.item, data.targetLocationId, this.facade.locations.flatLocations);
          this.facade.executor.run(
            this.facade.items.moveApi(data.item.id, data.targetLocationId),
            'ITEM_CARD.MODAL.MOVE_SUCCESS',
            () => this.jumpToLocation(data.targetLocationId),
            'SYSTEM.DEFAULT_ERROR'
          );
        }
      });
  }

  // --- Drag & Drop ---
  onLocationDragStart(loc: StorageLocation) { this.facade.locations.draggedLocationId = loc.id; }
  onLocationDragEnd() { this.facade.locations.draggedLocationId = null; }

  onRibbonReorder(event: CdkDragDrop<StorageLocation[]>) {
    const offset = this.facade.nav.currentPageRibbon * this.facade.nav.ribbonPageSize;
    moveItemInArray(this.facade.locations.locations, event.previousIndex + offset, event.currentIndex + offset);
    this.facade.executor.run(
      this.facade.locations.reorder(this.facade.locations.locations.map(l => l.id)),
      'LOCATIONS.SUCCESS.REORDER',
      () => this.loadData()
    );
  }

  onLocationMove(event: { loc: StorageLocation, targetId: string }) {
    const normalizedTargetId = event.targetId === 'root' ? null : event.targetId;
    this.facade.executor.run(this.facade.locations.move(event.loc.id, normalizedTargetId), 'LOCATIONS.SUCCESS.MOVE', () => this.loadData());
  }

  canDropRootLocation = (drag: CdkDrag): boolean => {
    const data = drag.data;
    if (!data || !('children' in data)) return false;
    return this.facade.tree.canMoveLocation(this.facade.locations.flatLocations, data.id, null);
  };

  onLocationDropped(data: { event: CdkDragDrop<StorageLocation[]>, targetId: string | null }) {
    const { event, targetId } = data;
    if (event.previousContainer === event.container) return;
    const loc = event.previousContainer.data[event.previousIndex];
    this.facade.executor.run(this.facade.locations.move(loc.id, targetId), 'LOCATIONS.SUCCESS.MOVE', () => this.loadData());
  }

  onItemDropped(data: { event: CdkDragDrop<Item[]>, loc: StorageLocation }) {
    const { event, loc } = data;
    if (!event.container.data) event.container.data = [];

    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }

    const item = event.previousContainer.data[event.previousIndex];
    this.facade.items.moveLocally(item, loc.id, this.facade.locations.flatLocations);
    this.facade.executor.run(this.facade.items.moveApi(item.id, loc.id), 'ITEM_CARD.TOASER.MOVE_SUCCESS', () => this.loadData());
  }

  trackById = (_: number, item: any) => item.id;
}