import { Component, OnInit, OnDestroy, inject, HostListener } from '@angular/core';
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
import { SellItemRequestDto } from '../../../models/dtos/sale.dto';
import { ItemStatus } from '../../../models/enums/item-status.enum';

@Component({
  selector: 'app-main-page',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    DragDropModule, 
    LocationCardComponent, 
    LocationRibbonComponent, 
    TranslateModule, 
    SellModalComponent
  ],
  templateUrl: './main-page.component.html',
  styleUrl: './main-page.component.scss'
})
export class MainPageComponent implements OnInit {
  private locationService = inject(LocationService);
  private itemService = inject(ItemService);
  private salesService = inject(SalesService);
  private router = inject(Router);

  locations: StorageLocation[] = [];
  flatLocations: StorageLocation[] = [];
  connectedLists: string[] = [];
  isLoading = true;
  itemToSell: Item | null = null;

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

  ngOnInit() { 
    this.loadData(); 
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    this.locations.forEach(loc => this.closeMenuRecursive(loc, event));
  }

  loadData() {
    this.isLoading = true;
    this.locationService.getTree().subscribe({
      next: (data: StorageLocation[]) => {
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

  // --- УПРАВЛЕНИЕ ЛОКАЦИЯМИ ---

  onRibbonReorder(event: CdkDragDrop<StorageLocation[]>) {
    const pageSize = window.innerWidth <= 768 ? 9 : 15;
    const offset = this.currentPageRibbon * pageSize;
    
    const globalPrev = event.previousIndex + offset;
    const globalCurr = event.currentIndex + offset;

    // Сдвигаем в локальном массиве
    moveItemInArray(this.locations, globalPrev, globalCurr);

    const payload = {
      parentId: null,
      orderedIds: this.locations.map(l => l.id)
    };

    this.locationService.reorder(payload).subscribe({
      error: () => {
        alert('Не удалось сохранить порядок локаций');
      }
    });
  }

  jumpToLocation(locId: string) {
    const index = this.locations.findIndex(l => l.id === locId);
    if (index !== -1) {
      this.currentPageBoard = Math.floor(index / this.pageSizeBoard);
    }
  }

  onLocationMove(event: { loc: StorageLocation, targetId: string }) {
    const newParentId = event.targetId === 'root' ? null : event.targetId;
    
    this.locationService.move(event.loc.id, newParentId).subscribe({
      next: () => {
        const targetLoc = this.findLocationById(this.locations, event.loc.id);
        if (!targetLoc) return;

        this.removeLocationFromTree(this.locations, event.loc.id);

        if (!newParentId) {
          this.locations.push(targetLoc);
        } else {
          const parent = this.findLocationById(this.locations, newParentId);
          if (parent) (parent.children ??= []).push(targetLoc);
        }
        
        this.refreshState();
        event.loc.showMenu = false;
      }
    });
  }

  onRename(loc: StorageLocation) {
    const newName = prompt('Введите новое имя локации', loc.name);
    if (newName && newName.trim() !== loc.name) {
      this.locationService.rename(loc.id, newName.trim()).subscribe({
        next: () => loc.name = newName.trim()
      });
    }
    loc.showMenu = false;
  }

  onDelete(loc: StorageLocation) {
    if (confirm(`Удалить локацию "${loc.name}" и всё её содержимое?`)) {
      this.locationService.delete(loc.id).subscribe({
        next: () => {
          this.removeLocationFromTree(this.locations, loc.id);
          this.locations = [...this.locations];
          this.refreshState();
          if (this.currentPageBoard > 0 && this.pagedBoardLocations.length === 0) {
            this.currentPageBoard--;
          }
        }
      });
    }
    loc.showMenu = false;
  }

  // --- УПРАВЛЕНИЕ ПРЕДМЕТАМИ ---

  onItemDropped(data: {event: CdkDragDrop<Item[]>, loc: StorageLocation}) {
    const { event, loc } = data;
    
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }

    const item = event.previousContainer.data[event.previousIndex];
    
    // Обновление статуса
    item.status = loc.isSalesLocation ? ItemStatus.Listed : loc.isLendingLocation ? ItemStatus.Lent : ItemStatus.Active;

    transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
    
    this.itemService.move(item.id, loc.id).subscribe();
  }

  onItemMoveManual(data: {item: Item, targetLocationId: string}) {
    const { item, targetLocationId } = data;
    const targetLoc = this.flatLocations.find(l => l.id === targetLocationId);
    
    if (!targetLoc) return;

    this.itemService.move(item.id, targetLocationId).subscribe({
      next: () => {
        const oldLoc = this.flatLocations.find(l => l.id === item.storageLocationId);
        if (oldLoc?.items) {
          oldLoc.items = oldLoc.items.filter(i => i.id !== item.id);
        }
        
        item.storageLocationId = targetLoc.id;
        item.status = targetLoc.isSalesLocation ? ItemStatus.Listed : targetLoc.isLendingLocation ? ItemStatus.Lent : ItemStatus.Active;

        (targetLoc.items ??= []).push(item);
        
        this.locations = [...this.locations]; 
        this.refreshState();

        if (!this.activeBoardIds.includes(targetLocationId)) {
          if (confirm('Предмет перемещен. Перейти к новой локации?')) {
            this.jumpToLocation(targetLocationId);
          }
        }
      }
    });
  }

  onDeleteItem(item: Item) {
    if (confirm(`Вы уверены, что хотите удалить "${item.name}"?`)) {
      this.itemService.delete(item.id).subscribe({
        next: () => {
          const location = this.flatLocations.find(l => l.id === item.storageLocationId);
          if (location && location.items) {
            location.items = location.items.filter(i => i.id !== item.id);           
            this.locations = [...this.locations]; 
          }
        }
      });
    }
  }

  // --- ПРОДАЖИ ---

  onSellRequest(item: Item) { 
    this.itemToSell = item; 
  }

  onSellConfirmed(dto: SellItemRequestDto) {
    this.salesService.sellItem(dto).subscribe({
      next: () => { 
        this.itemToSell = null; 
        this.router.navigate(['/sales']); 
      }
    });
  }

  // --- ХЕЛПЕРЫ ---

  isChildOf(targetId: string, sourceLoc: StorageLocation): boolean {
    return sourceLoc.children?.some(c => c.id === targetId || this.isChildOf(targetId, c)) || false;
  }

  private findLocationById(tree: StorageLocation[], id: string): StorageLocation | undefined {
    for (const loc of tree) {
      if (loc.id === id) return loc;
      const found = loc.children && this.findLocationById(loc.children, id);
      if (found) return found;
    }
    return undefined;
  }

  private removeLocationFromTree(tree: StorageLocation[], id: string): boolean {
    for (let i = 0; i < tree.length; i++) {
      if (tree[i].id === id) {
        tree.splice(i, 1);
        return true;
      }
      if (tree[i].children?.length) {
        if (this.removeLocationFromTree(tree[i].children!, id)) return true;
      }
    }
    return false;
  }

  private closeMenuRecursive(loc: StorageLocation, event: MouseEvent) {
    if (loc.showMenu) {
      const target = event.target as HTMLElement;
      const isInside = target.closest('.menu-dropdown') || target.closest('.inest-action-btn');
      if (!isInside) loc.showMenu = false;
    }
    loc.children?.forEach(c => this.closeMenuRecursive(c, event));
  }

  trackById = (index: number, item: any) => item.id;
}