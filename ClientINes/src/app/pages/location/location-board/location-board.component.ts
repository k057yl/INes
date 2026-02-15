import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { environment } from '../../../../environments/environment';
import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { Item } from '../../../models/entities/item.entity';
import { LocationCardComponent } from '../location-card/location-card.component';

@Component({
  selector: 'app-location-board',
  standalone: true,
  imports: [CommonModule, RouterModule, DragDropModule, LocationCardComponent],
  template: `
    <div class="root-ribbon-container">
      <div class="ribbon-label">Порядок колонок:</div>
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
        <i class="fa fa-plus"></i> Добавить локацию
      </button>
    </div>

    <div class="board-wrapper">
      <div class="loader" *ngIf="isLoading">Загружаем...</div>

      <app-location-card 
        *ngFor="let loc of locations; trackBy: trackById"
        [location]="loc"
        [flatLocations]="flatLocations"
        [connectedLists]="connectedLists"
        [isChildOf]="isChildOf.bind(this)"
        (itemDropped)="onItemDropped($event)"
        (move)="onLocationMove($event)"
        (rename)="onRename($event)"
        (delete)="onDelete($event)">
      </app-location-card>
    </div>
  `,
  styles: [`
    .root-ribbon-container {
      background: #f8fafc;
      padding: 10px 20px;
      border-bottom: 1px solid #e2e8f0;
      display: flex;
      align-items: center;
      gap: 15px;
    }
    .ribbon-label { font-size: 0.8rem; font-weight: bold; color: #64748b; text-transform: uppercase; }
    
    .root-ribbon { display: flex; gap: 10px; }
    
    .root-chip {
      background: white;
      padding: 6px 12px;
      border-radius: 20px;
      border: 1px solid #cbd5e0;
      font-size: 0.85rem;
      cursor: grab;
      display: flex;
      align-items: center;
      gap: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.05);
    }
    .root-chip:active { cursor: grabbing; }

    .board-wrapper { 
      display: flex; gap: 20px; padding: 20px; overflow-x: auto; align-items: flex-start; 
      min-height: calc(100vh - 150px);
    }
    .header-actions { padding: 20px 20px 0 20px; }
    .add-loc-btn { 
      padding: 10px 20px; background: #28a745; color: white; border-radius: 8px; cursor: pointer; border: none; font-weight: 600;
    }
    .chip-placeholder { background: #edf2fd; border: 1px dashed #3182ce; border-radius: 20px; width: 80px; }
  `]
})
export class LocationBoardComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  locations: StorageLocation[] = [];
  flatLocations: StorageLocation[] = [];
  connectedLists: string[] = [];
  isLoading = true;

  private documentClickHandler = (e: MouseEvent) => this.onDocumentClick(e);

  ngOnInit() {
    this.loadData();
    document.addEventListener('click', this.documentClickHandler);
  }

  ngOnDestroy() {
    document.removeEventListener('click', this.documentClickHandler);
  }

  trackById = (index: number, item: any) => item.id;

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

  onLocationReordered(data: { event: CdkDragDrop<StorageLocation[]>, parentId: string | null }) {
    this.onLocationDropped(data.event, data.parentId);
  }

  onLocationDropped(event: CdkDragDrop<StorageLocation[]>, parentId: string | null) {
    if (event.previousIndex === event.currentIndex) return;

    const targetArray = parentId === null 
      ? this.locations 
      : this.findLocationById(this.locations, parentId)?.children;

    if (targetArray) {
      moveItemInArray(targetArray, event.previousIndex, event.currentIndex);
      const orderedIds = targetArray.map(l => l.id);
      
      this.http.patch(`${environment.apiBaseUrl}/locations/reorder`, {
        parentId: parentId,
        orderedIds: orderedIds
      }).subscribe({
        error: (err) => {
          console.error('Не удалось сохранить порядок:', err);
        }
      });
    }
  }

  onLocationMove(event: { loc: StorageLocation, targetId: string }) {
    const newParentId = event.targetId === 'root' ? null : event.targetId;
    this.http.patch(`${environment.apiBaseUrl}/locations/${event.loc.id}/move`, { newParentId }).subscribe({
      next: () => this.moveLocationLocally(event.loc.id, newParentId)
    });
  }

  onItemDropped(data: {event: CdkDragDrop<Item[]>, loc: StorageLocation}) {
    const { event, loc } = data;
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const item = event.previousContainer.data[event.previousIndex];
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
      this.http.patch(`${environment.apiBaseUrl}/items/${item.id}/move`, { targetLocationId: loc.id }).subscribe();
    }
  }

  isChildOf(targetId: string, sourceLoc: StorageLocation): boolean {
    return sourceLoc.children?.some(c => c.id === targetId || this.isChildOf(targetId, c)) || false;
  }

  private moveLocationLocally(locId: string, newParentId: string | null) {
    const targetLoc = this.findLocationById(this.locations, locId);
    if (!targetLoc) return;
    this.removeLocationFromTree(this.locations, locId);
    if (!newParentId) this.locations.push(targetLoc);
    else {
      const parent = this.findLocationById(this.locations, newParentId);
      if (parent) { (parent.children ??= []).push(targetLoc); }
    }
    this.refreshState();
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
      this.http.patch(`${environment.apiBaseUrl}/locations/${loc.id}/rename`, { name: newName.trim() }).subscribe({
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
      if (btn && !btn.contains(event.target as Node)) loc.showMenu = false;
    }
    loc.children?.forEach(c => this.closeMenuRecursive(c, event));
  }
}