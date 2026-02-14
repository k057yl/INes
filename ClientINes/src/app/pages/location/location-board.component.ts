import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { environment } from '../../../environments/environment';
import { StorageLocation } from '../../models/entities/storage-location.entity';
import { Item } from '../../models/entities/item.entity';
import { LocationCardComponent } from './location-card/location-card.component';

@Component({
  selector: 'app-location-board',
  standalone: true,
  imports: [CommonModule, RouterModule, DragDropModule, LocationCardComponent],
  template: `
    <div class="header-actions">
      <button routerLink="/location-create" class="add-loc-btn">
        + Добавить локацию
      </button>
    </div>

    <div class="board-wrapper" 
         cdkDropList 
         cdkDropListOrientation="horizontal" 
         [cdkDropListData]="locations"
         (cdkDropListDropped)="onLocationDropped($event, null)">
      
      <div class="loader" *ngIf="isLoading">Загружаем локации...</div>

      <app-location-card 
        *ngFor="let loc of locations; trackBy: trackById"
        cdkDrag
        [cdkDragData]="loc"
        [location]="loc"
        [flatLocations]="flatLocations"
        [connectedLists]="connectedLists"
        [isChildOf]="isChildOf.bind(this)"
        (itemDropped)="onItemDropped($event)"
        (locationReordered)="onLocationReordered($event)"
        (move)="onLocationMove($event)"
        (rename)="onRename($event)"
        (delete)="onDelete($event)">
        
        <div class="location-placeholder" *cdkDragPlaceholder></div>
      </app-location-card>
    </div>
  `,
  styles: [`
    .board-wrapper { 
      display: flex; gap: 20px; padding: 20px; flex-wrap: wrap; align-items: flex-start; 
      min-height: 400px;
    }
    .header-actions { padding: 20px; }
    .add-loc-btn { 
      padding: 10px 20px; background: #28a745; color: white; border-radius: 5px; cursor: pointer; border: none; 
    }
    .location-placeholder {
      background: rgba(0,0,0,0.05);
      border: 2px dashed #ccc;
      border-radius: 8px;
      width: 280px;
      height: 200px;
    }
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