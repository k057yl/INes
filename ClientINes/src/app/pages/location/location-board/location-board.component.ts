import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';

import { environment } from '../../../../environments/environment';
import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { Item } from '../../../models/entities/item.entity';
import { LocationCardComponent } from '../location-card/location-card.component';
import { SalesService } from '../../../services/sales.service';
import { SellModalComponent } from '../../sell-modal/sell-modal.component';
import { SellItemRequestDto } from '../../../models/dtos/sale.dto';

@Component({
  selector: 'app-location-board',
  standalone: true,
  imports: [CommonModule, RouterModule, DragDropModule, LocationCardComponent, TranslateModule, SellModalComponent],
  template: `
    <div class="root-ribbon-container">
      <div class="ribbon-label">{{ 'BOARD.COLUMN_ORDER' | translate }}</div>
      <div class="root-ribbon" 
           cdkDropList 
           cdkDropListOrientation="horizontal" 
           [cdkDropListData]="locations"
           (cdkDropListDropped)="onLocationDropped($event, null)">
        
        <div class="root-chip" *ngFor="let loc of locations" cdkDrag>
          <i [className]="'fa ' + (loc.icon || 'fa-folder')" [style.color]="loc.color"></i>
          {{ loc.name }}
          <div class="chip-placeholder" *cdkDragPlaceholder></div>
        </div>
      </div>
    </div>

    <div class="header-actions">
      <button routerLink="/location-create" class="add-loc-btn">
        <i class="fa fa-plus"></i> {{ 'BOARD.ADD_LOCATION' | translate }}
      </button>
      <button routerLink="/create-item" class="add-item-btn">
        <i class="fa fa-box-open"></i> Добавить предмет
      </button>
    </div>

    <div class="board-wrapper">
      <div class="loader" *ngIf="isLoading">{{ 'COMMON.LOADING' | translate }}</div>

      <app-location-card 
        *ngFor="let loc of locations; trackBy: trackById"
        [location]="loc"
        [flatLocations]="flatLocations"
        [connectedLists]="connectedLists"
        [isChildOf]="isChildOf.bind(this)"
        (itemDropped)="onItemDropped($event)"
        (sellItem)="onSellRequest($event)" 
        (move)="onLocationMove($event)"
        (rename)="onRename($event)"
        (delete)="onDelete($event)">
      </app-location-card>
    </div>

    <app-sell-modal 
      *ngIf="itemToSell" 
      [item]="itemToSell"
      (close)="itemToSell = null"
      (confirm)="onSellConfirmed($event)">
    </app-sell-modal>
  `,
  styles: [`
    :host {
      --bg-dark: #0b132b;
      --bg-navy: #1c2541;
      --accent-teal: #00f5d4;
      --text-muted: #94a3b8;
      display: block;
      background: var(--bg-dark);
      min-height: 100vh;
    }
    .root-ribbon-container {
      background: var(--bg-navy);
      padding: 12px 20px;
      border-bottom: 1px solid #3a506b;
      display: flex;
      align-items: center;
      gap: 15px;
    }
    .ribbon-label { font-size: 0.75rem; font-weight: bold; color: var(--text-muted); text-transform: uppercase; }
    .root-chip {
      background: var(--bg-dark); padding: 8px 16px; border-radius: 20px;
      border: 1px solid #3a506b; color: white; font-size: 0.85rem;
      cursor: grab; display: flex; align-items: center; gap: 8px;
    }
    .header-actions { padding: 20px; }
    .add-loc-btn { 
      padding: 12px 24px; background: var(--bg-navy); color: var(--accent-teal); 
      border: 1px solid var(--accent-teal); border-radius: 8px; cursor: pointer; 
      font-weight: 600; transition: all 0.3s;
    }
    .add-loc-btn:hover { background: var(--accent-teal); color: var(--bg-dark); }
    .board-wrapper { display: flex; gap: 20px; padding: 20px; overflow-x: auto; align-items: flex-start; }
    .loader { color: var(--accent-teal); padding: 20px; }
    .add-item-btn { 
      margin-left: 15px;
      padding: 12px 24px; 
      background: var(--accent-teal); 
      color: var(--bg-dark); 
      border: none; 
      border-radius: 8px; 
      cursor: pointer; 
      font-weight: 600; 
      transition: all 0.3s;
    }
    .add-item-btn:hover { 
      transform: translateY(-2px);
      box-shadow: 0 0 15px var(--accent-teal);
    }
  `]
})
export class LocationBoardComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private salesService = inject(SalesService);
  private router = inject(Router);

  locations: StorageLocation[] = [];
  flatLocations: StorageLocation[] = [];
  connectedLists: string[] = [];
  isLoading = true;
  
  itemToSell: Item | null = null;

  private documentClickHandler = (e: MouseEvent) => this.onDocumentClick(e);

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

  onSellRequest(item: Item) {
    this.itemToSell = item;
  }

  closeSellModal() {
    this.itemToSell = null;
  }

  onSellConfirmed(dto: SellItemRequestDto) {
    this.salesService.sellItem(dto).subscribe({
      next: (res) => {
        console.log(`Продано! Профит: ${res.profit}$`);
        
        this.closeSellModal();
        
        this.router.navigate(['/sales']);
      },
      error: err => {
        if (err.status === 400) {
          alert('Этот предмет уже продан!');
        }
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

    transferArrayItem(
      event.previousContainer.data,
      event.container.data,
      event.previousIndex,
      event.currentIndex
    );

    this.http.patch(`${environment.apiBaseUrl}/items/${item.id}/move`, { 
      targetLocationId: loc.id 
    }).subscribe({
      error: (err) => {
        console.error('Ошибка перемещения:', err);
        this.loadData();
      }
    });
  }

  onLocationDropped(event: CdkDragDrop<StorageLocation[]>, parentId: string | null) {
    if (event.previousIndex === event.currentIndex) return;
    const targetArray = parentId === null 
      ? this.locations 
      : this.findLocationById(this.locations, parentId)?.children;

    if (targetArray) {
      moveItemInArray(targetArray, event.previousIndex, event.currentIndex);
      this.http.patch(`${environment.apiBaseUrl}/locations/reorder`, {
        parentId,
        orderedIds: targetArray.map(l => l.id)
      }).subscribe();
    }
  }

  onLocationMove(event: { loc: StorageLocation, targetId: string }) {
    const newParentId = event.targetId === 'root' ? null : event.targetId;
    this.http.patch(`${environment.apiBaseUrl}/locations/${event.loc.id}/move`, { newParentId }).subscribe({
      next: () => this.moveLocationLocally(event.loc.id, newParentId),
      error: (err) => console.error('Ошибка перемещения локации:', err)
    });
  }

  private moveLocationLocally(locId: string, newParentId: string | null) {
    const targetLoc = this.findLocationById(this.locations, locId);
    if (!targetLoc) return;
    this.removeLocationFromTree(this.locations, locId);
    if (!newParentId) {
      this.locations.push(targetLoc);
    } else {
      const parent = this.findLocationById(this.locations, newParentId);
      if (parent) (parent.children ??= []).push(targetLoc);
    }
    this.refreshState();
  }

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
      if (tree[i].id === id) { tree.splice(i, 1); return true; }
      if (tree[i].children && this.removeLocationFromTree(tree[i].children!, id)) return true;
    }
    return false;
  }

  onRename(loc: StorageLocation) {
    const newName = prompt('Введите новое имя локации', loc.name);
    if (newName && newName.trim() !== loc.name) {
      this.http.patch(`${environment.apiBaseUrl}/locations/${loc.id}/rename`, { 
        name: newName.trim() 
      }).subscribe({
        next: () => loc.name = newName.trim(),
        error: (err) => console.error('Ошибка переименования:', err)
      });
    }
    loc.showMenu = false;
  }

  onDelete(loc: StorageLocation) {
    if (confirm(`Удалить локацию "${loc.name}" и всё её содержимое?`)) {
      this.http.delete(`${environment.apiBaseUrl}/locations/${loc.id}`).subscribe({
        next: () => this.loadData(),
        error: (err) => console.error('Ошибка удаления:', err)
      });
    }
    loc.showMenu = false;
  }

  private onDocumentClick(event: MouseEvent) {
    this.locations.forEach(loc => this.closeMenuRecursive(loc, event));
  }

  private closeMenuRecursive(loc: StorageLocation, event: MouseEvent) {
    if (loc.showMenu) {
      const btn = document.getElementById('btn-' + loc.id);
      if (btn && !btn.contains(event.target as Node)) {
        loc.showMenu = false;
      }
    }
    loc.children?.forEach(c => this.closeMenuRecursive(c, event));
  }

  trackById = (index: number, item: any) => item.id;
}