import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';

import { environment } from '../../../environments/environment';
import { StorageLocation } from '../../models/entities/storage-location.entity';
import { Item } from '../../models/entities/item.entity';
import { LocationCardComponent } from '../../components/location-card/location-card.component';
import { LocationRibbonComponent } from '../../components/location-ribbon/location-ribbon.component';
import { SalesService } from '../../services/sales.service';
import { SellModalComponent } from '../../components/sell-modal/sell-modal.component';
import { SellItemRequestDto } from '../../models/dtos/sale.dto';

@Component({
  selector: 'app-main-page',
  standalone: true,
  imports: [CommonModule, RouterModule, DragDropModule, LocationCardComponent, LocationRibbonComponent, TranslateModule, SellModalComponent],
  templateUrl: './main-page.component.html',
  styleUrl: './main-page.component.css'
})
export class MainPageComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
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

  private documentClickHandler = (e: MouseEvent) => this.onDocumentClick(e);

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
    document.addEventListener('click', this.documentClickHandler);
  }

  ngOnDestroy() {
    document.removeEventListener('click', this.documentClickHandler);
  }

  loadData() {
    this.isLoading = true;
    this.http.get<StorageLocation[]>(`${environment.apiBaseUrl}/locations/tree`).subscribe({
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

  flattenLocations(locs: StorageLocation[]): StorageLocation[] {
    return locs.reduce<StorageLocation[]>((acc, l) => {
      acc.push(l);
      if (l.children?.length) acc.push(...this.flattenLocations(l.children));
      return acc;
    }, []);
  }

  // --- УПРАВЛЕНИЕ ЛОКАЦИЯМИ ---
  onRibbonReorder(event: CdkDragDrop<StorageLocation[]>) {
    const offset = this.currentPageRibbon * this.pageSizeRibbon;
    const globalPrev = event.previousIndex + offset;
    const globalCurr = event.currentIndex + offset;

    moveItemInArray(this.locations, globalPrev, globalCurr);
    this.http.patch(`${environment.apiBaseUrl}/locations/reorder`, {
      parentId: null,
      orderedIds: this.locations.map(l => l.id)
    }).subscribe();
  }

  jumpToLocation(locId: string) {
    const index = this.locations.findIndex(l => l.id === locId);
    if (index !== -1) {
      this.currentPageBoard = Math.floor(index / this.pageSizeBoard);
    }
  }

  onLocationMove(event: { loc: StorageLocation, targetId: string }) {
    const newParentId = event.targetId === 'root' ? null : event.targetId;
    this.http.patch(`${environment.apiBaseUrl}/locations/${event.loc.id}/move`, { newParentId }).subscribe({
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
      }
    });
  }

  onRename(loc: StorageLocation) {
    const newName = prompt('Введите новое имя локации', loc.name);
    if (newName && newName.trim() !== loc.name) {
      this.http.patch(`${environment.apiBaseUrl}/locations/${loc.id}/rename`, { name: newName.trim() }).subscribe({
        next: () => loc.name = newName.trim()
      });
    }
    loc.showMenu = false;
  }

  onDelete(loc: StorageLocation) {
    if (confirm(`Удалить локацию "${loc.name}" и всё её содержимое?`)) {
        this.http.delete(`${environment.apiBaseUrl}/locations/${loc.id}`).subscribe({
        next: () => {
            this.removeLocationFromTree(this.locations, loc.id);

            this.locations = [...this.locations];
            this.refreshState();

            if (this.currentPageBoard > 0 && this.pagedBoardLocations.length === 0) {
            this.currentPageBoard--;
            }
        },
        error: (err) => console.error('Ошибка при удалении локации:', err)
        });
    }
    loc.showMenu = false;
    }

    /**
     * Рекурсивный помощник для удаления локации из дерева
     */
    private removeLocationFromTree(tree: StorageLocation[], id: string): boolean {
    for (let i = 0; i < tree.length; i++) {
        if (tree[i].id === id) {
        tree.splice(i, 1);
        return true;
        }
        if (tree[i].children && tree[i].children!.length > 0) {
        const deleted = this.removeLocationFromTree(tree[i].children!, id);
        if (deleted) return true;
        }
    }
    return false;
    }

  // --- УПРАВЛЕНИЕ ПРЕДМЕТАМИ ---
  onItemDropped(data: {event: CdkDragDrop<Item[]>, loc: StorageLocation}) {
    const { event, loc } = data;
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }
    const item = event.previousContainer.data[event.previousIndex];
    item.status = loc.isSalesLocation ? 6 : loc.isLendingLocation ? 1 : 0;

    transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
    this.http.patch(`${environment.apiBaseUrl}/items/${item.id}/move`, { targetLocationId: loc.id }).subscribe();
  }

  onSellRequest(item: Item) { this.itemToSell = item; }

  onSellConfirmed(dto: SellItemRequestDto) {
    this.salesService.sellItem(dto).subscribe({
      next: () => { this.itemToSell = null; this.router.navigate(['/sales']); }
    });
  }

  // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---
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

  private onDocumentClick(event: MouseEvent) {
    this.locations.forEach(loc => this.closeMenuRecursive(loc, event));
  }

  private closeMenuRecursive(loc: StorageLocation, event: MouseEvent) {
    if (loc.showMenu) {
      const btn = document.getElementById('btn-' + loc.id);
      if (btn && !btn.contains(event.target as Node)) loc.showMenu = false;
    }
    loc.children?.forEach(c => this.closeMenuRecursive(c, event));
  }

  onDeleteItem(item: Item) {
    if (confirm(`Вы уверены, что хотите удалить "${item.name}"?`)) {
        this.http.delete(`${environment.apiBaseUrl}/items/${item.id}`).subscribe({
        next: () => {
            const location = this.flatLocations.find(l => l.id === item.storageLocationId);
            
            if (location && location.items) {
            location.items = location.items.filter(i => i.id !== item.id);           
            this.locations = [...this.locations]; 
            }
        },
        error: (err) => console.error('Ошибка при удалении:', err)
        });
    }
    }

  trackById = (index: number, item: any) => item.id;
}