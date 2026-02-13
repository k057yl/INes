import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { 
  CdkDragDrop, 
  CdkDropListGroup, 
  CdkDropList, 
  CdkDrag, 
  CdkDragPlaceholder,
  moveItemInArray, 
  transferArrayItem 
} from '@angular/cdk/drag-drop';
import { environment } from '../../../environments/environment';

interface Item {
  id: string;
  name: string;
}

interface StorageLocation {
  id: string;
  name: string;
  color: string;
  icon: string;
  items: Item[];
  children?: StorageLocation[];
  showMenu?: boolean;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CdkDropList,
    CdkDrag,
    CdkDragPlaceholder
  ],
  template: `
    <div class="header-actions">
      <button routerLink="/location-create" class="add-loc-btn">
        + Добавить локацию
      </button>
    </div>

    <div class="board-wrapper">
      <div class="loader" *ngIf="isLoading">Загружаем локации...</div>

      <ng-container *ngFor="let loc of locations; trackBy: trackById">
        <ng-container *ngTemplateOutlet="locationNode; context:{ $implicit: loc }"></ng-container>
      </ng-container>

      <ng-template #locationNode let-loc>
        <div class="location-column" [style.border-top-color]="loc.color">
          <div class="loc-header">
            <i [className]="'fa ' + (loc.icon || 'fa-folder')"></i>
            <h3>{{ loc.name }}</h3>
            <button [id]="'btn-' + loc.id" (click)="toggleMenu(loc, $event)">•••</button>

            <div class="menu" [id]="'menu-' + loc.id" *ngIf="loc.showMenu">
              <div class="menu-section">
                <label>Переместить в:</label>
                <select (change)="onSelectTarget(loc, $event)">
                  <option value="" disabled selected>Выберите цель</option>
                  <option value="root">В корень (верхний уровень)</option>
                  <option *ngFor="let target of flatLocations; trackBy: trackById" 
                          [value]="target.id" 
                          [disabled]="target.id === loc.id || isChildOf(target.id, loc)">
                    {{ target.name }} {{ target.id === loc.id ? '(это она)' : '' }}
                  </option>
                </select>
              </div>
              <hr>
              <div class="menu-actions">
                <button (click)="renameLocation(loc)">Переименовать</button>
                <button (click)="deleteLocation(loc)" class="delete-btn">Удалить</button>
              </div>
            </div>
          </div>

          <div
            cdkDropList
            [id]="loc.id"
            [cdkDropListData]="loc.items"
            [cdkDropListConnectedTo]="connectedLists"
            (cdkDropListDropped)="dropItem($event, loc)"
            class="items-container">
            <div class="item-card" *ngFor="let item of loc.items; trackBy: trackById" cdkDrag>
              {{ item.name }}
              <div class="item-placeholder" *cdkDragPlaceholder></div>
            </div>
            <div class="empty-state" *ngIf="loc.items.length === 0">Пусто</div>
          </div>

          <div class="children" *ngIf="loc.children?.length">
            <ng-container *ngFor="let child of loc.children; trackBy: trackById">
              <ng-container *ngTemplateOutlet="locationNode; context:{ $implicit: child }"></ng-container>
            </ng-container>
          </div>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .header-actions { padding: 20px; }
    .add-loc-btn { padding: 10px 20px; background: #28a745; color: white; border: none; border-radius: 5px; cursor: pointer; }
    .board-wrapper { display: flex; gap: 20px; padding: 20px; flex-wrap: wrap; align-items: flex-start; }
    .location-column { background: #ebedf0; border-radius: 8px; width: 280px; flex-shrink: 0; border-top: 5px solid #007bff; display: flex; flex-direction: column; padding: 10px; margin-bottom: 20px; }
    .loc-header { padding: 10px; display: flex; align-items: center; gap: 10px; justify-content: space-between; position: relative; }
    .menu { position: absolute; top: 35px; right: 0; background: white; border: 1px solid #ccc; padding: 12px; border-radius: 8px; z-index: 100; box-shadow: 0 4px 12px rgba(0,0,0,0.15); min-width: 200px; }
    .menu-section { margin-bottom: 10px; }
    .menu label { display: block; font-size: 0.8rem; color: #666; margin-bottom: 4px; }
    .menu select { width: 100%; padding: 4px; margin-bottom: 8px; }
    .menu-actions button { display: block; width: 100%; text-align: left; padding: 6px; background: none; border: none; cursor: pointer; border-radius: 4px; }
    .menu-actions button:hover { background: #f0f0f0; }
    .delete-btn { color: #dc3545; }
    .items-container { min-height: 80px; padding: 10px; background: #f8f9fa; border-radius: 4px; }
    .item-card { background: white; padding: 8px; margin-bottom: 6px; border-radius: 4px; box-shadow: 0 1px 2px rgba(0,0,0,0.1); cursor: grab; }
    .item-placeholder { background: #ccc; border: 2px dashed #999; height: 36px; margin-bottom: 6px; }
    .children { padding-left: 15px; margin-top: 10px; border-left: 2px solid #ddd; }
  `]
})
export class HomeComponent implements OnInit, OnDestroy {
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

  trackById(index: number, item: any): string {
    return item.id;
  }

  loadData() {
    this.isLoading = true;
    this.http.get<StorageLocation[]>(`${environment.apiBaseUrl}/locations/tree`).subscribe({
      next: (data) => {
        this.locations = data;
        this.refreshState();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Ошибка загрузки:', err);
        this.isLoading = false;
      }
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

  isChildOf(targetId: string, sourceLoc: StorageLocation): boolean {
    if (!sourceLoc.children?.length) return false;
    
    for (const child of sourceLoc.children) {
      if (child.id === targetId) return true;
      if (this.isChildOf(targetId, child)) return true;
    }
    return false;
  }

  onSelectTarget(loc: StorageLocation, event: Event) {
    const targetId = (event.target as HTMLSelectElement).value;
    const newParentId = targetId === 'root' ? null : targetId;

    loc.showMenu = false;

    this.http.patch(`${environment.apiBaseUrl}/locations/${loc.id}/move`, { newParentId }).subscribe({
      next: () => {
        this.moveLocationLocally(loc.id, newParentId);
      },
      error: (err) => {
        console.error('Не удалось переместить локацию:', err);
        alert('Ошибка при перемещении');
      }
    });
  }

  moveLocationLocally(locId: string, newParentId: string | null) {
    const targetLoc = this.findLocationById(this.locations, locId);
    if (!targetLoc) return;

    this.removeLocationFromTree(this.locations, locId);

    if (!newParentId) {
      this.locations.push(targetLoc);
    } else {
      const parent = this.findLocationById(this.locations, newParentId);
      if (parent) {
        parent.children = parent.children || [];
        parent.children.push(targetLoc);
      }
    }
    this.refreshState();
  }

  onDocumentClick(event: MouseEvent) {
    this.locations.forEach(loc => this.closeMenuRecursive(loc, event));
  }

  closeMenuRecursive(loc: StorageLocation, event: MouseEvent) {
    if (loc.showMenu) {
      const menuEl = document.getElementById('menu-' + loc.id);
      const btnEl = document.getElementById('btn-' + loc.id);
      if (menuEl && !menuEl.contains(event.target as Node) && btnEl && !btnEl.contains(event.target as Node)) {
        loc.showMenu = false;
      }
    }
    loc.children?.forEach(c => this.closeMenuRecursive(c, event));
  }

  toggleMenu(loc: StorageLocation, event: MouseEvent) {
    event.stopPropagation();
    loc.showMenu = !loc.showMenu;
  }

  removeLocationFromTree(tree: StorageLocation[], id: string): boolean {
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

  findLocationById(tree: StorageLocation[], id: string): StorageLocation | undefined {
    for (const loc of tree) {
      if (loc.id === id) return loc;
      if (loc.children?.length) {
        const found = this.findLocationById(loc.children, id);
        if (found) return found;
      }
    }
    return undefined;
  }

  renameLocation(loc: StorageLocation) {
    const newName = prompt('Введите новое имя локации', loc.name);
    if (newName && newName.trim() !== loc.name) {
      this.http.patch(`${environment.apiBaseUrl}/locations/${loc.id}/rename`, { name: newName.trim() }).subscribe({
        next: () => loc.name = newName.trim(),
        error: (err) => console.error('Ошибка переименования:', err)
      });
    }
    loc.showMenu = false;
  }

  deleteLocation(loc: StorageLocation) {
    if (confirm(`Удалить локацию "${loc.name}" и всё её содержимое?`)) {
      this.http.delete(`${environment.apiBaseUrl}/locations/${loc.id}`).subscribe({
        next: () => this.loadData(),
        error: (err) => console.error('Ошибка удаления:', err)
      });
    }
    loc.showMenu = false;
  }

  dropItem(event: CdkDragDrop<Item[]>, loc: StorageLocation) {
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
}