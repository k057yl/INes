import { Injectable } from '@angular/core';
import { StorageLocation } from '../../location/contracts/storage-location';
import { RIBBON_CONFIG, BOARD_CONFIG } from '../../../shared/constants/ui.constants';

@Injectable()
export class DashboardNavigationService {
  currentPageBoard = 0;
  currentPageRibbon = 0;

  get ribbonPageSize(): number {
    return window.innerWidth <= RIBBON_CONFIG.BREAKPOINT_MOBILE
      ? RIBBON_CONFIG.PAGE_SIZE_MOBILE
      : RIBBON_CONFIG.PAGE_SIZE_DESKTOP;
  }

  get boardPageSize(): number {
    return window.innerWidth <= BOARD_CONFIG.BREAKPOINT_MOBILE
      ? BOARD_CONFIG.PAGE_SIZE_MOBILE
      : BOARD_CONFIG.PAGE_SIZE_DESKTOP;
  }

  getBoardPageLocations(locations: StorageLocation[]): StorageLocation[] {
    const start = this.currentPageBoard * this.boardPageSize;
    return locations.slice(start, start + this.boardPageSize);
  }

  getTotalBoardPages(locationsCount: number): number {
    return Math.max(1, Math.ceil(locationsCount / this.boardPageSize));
  }

  changeBoardPage(delta: number, locationsCount: number) {
    const newPage = this.currentPageBoard + delta;
    const maxPages = this.getTotalBoardPages(locationsCount);

    if (newPage >= 0 && newPage < maxPages) {
      this.currentPageBoard = newPage;
      this.syncRibbonWithBoard();
    }
  }

  onRibbonPageChange(newPage: number) {
    this.currentPageRibbon = newPage;
    const firstItemIndex = newPage * this.ribbonPageSize;
    this.currentPageBoard = Math.floor(firstItemIndex / this.boardPageSize);
  }

  jumpToLocation(locId: string, locations: StorageLocation[]) {
    const index = locations.findIndex(l => l.id === locId);
    if (index !== -1) {
      this.currentPageBoard = Math.floor(index / this.boardPageSize);
      this.syncRibbonWithBoard();
    }
  }

  syncRibbonWithBoard() {
    const firstVisibleIndex = this.currentPageBoard * this.boardPageSize;
    this.currentPageRibbon = Math.floor(firstVisibleIndex / this.ribbonPageSize);
  }

  adjustPageAfterDelete(pagedCount: number) {
    if (this.currentPageBoard > 0 && pagedCount === 0) {
      this.currentPageBoard--;
    }
    this.syncRibbonWithBoard();
  }
}