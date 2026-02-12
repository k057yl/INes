import { Component, OnInit, inject } from '@angular/core';
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
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    CdkDropListGroup, 
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

    <div class="board-wrapper" cdkDropListGroup>
      <div class="loader" *ngIf="isLoading">Загружаем локации...</div>

      <div class="location-column" *ngFor="let loc of locations" [style.border-top-color]="loc.color">
        <div class="loc-header">
          <i [className]="'fa ' + (loc.icon || 'fa-folder')"></i>
          <h3>{{ loc.name }}</h3>
        </div>

        <div
          cdkDropList
          [id]="loc.id"
          [cdkDropListData]="loc.items"
          class="items-container"
          (cdkDropListDropped)="drop($event)">
          
          <div class="item-card" *ngFor="let item of loc.items" cdkDrag>
            {{ item.name }}
            <div class="item-placeholder" *cdkDragPlaceholder></div>
          </div>

          <div class="empty-state" *ngIf="loc.items.length === 0">
            Пусто
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .header-actions { padding: 20px; }
    .add-loc-btn { 
      padding: 10px 20px; background: #28a745; color: white; border: none; border-radius: 5px; cursor: pointer;
    }
    .board-wrapper { 
      display: flex; gap: 20px; padding: 20px; overflow-x: auto; align-items: flex-start;
    }
    .location-column { 
      background: #ebedf0; border-radius: 8px; width: 280px; flex-shrink: 0;
      border-top: 5px solid #007bff; display: flex; flex-direction: column;
    }
    .loc-header { padding: 15px; display: flex; align-items: center; gap: 10px; }
    .loc-header h3 { margin: 0; font-size: 1.1rem; }
    .items-container { min-height: 100px; padding: 10px; }
    .item-card { 
      background: white; padding: 12px; margin-bottom: 8px; border-radius: 4px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1); cursor: grab;
    }
    .item-placeholder { background: #ccc; border: 2px dashed #999; height: 40px; margin-bottom: 8px; }
    .empty-state { color: #888; text-align: center; padding: 20px; font-size: 0.9rem; }
  `]
})
export class HomeComponent implements OnInit {
  private http = inject(HttpClient);
  
  locations: StorageLocation[] = [];
  isLoading = true;

  ngOnInit() {

  setTimeout(() => {
    this.loadData();
  }, 50);
}

  loadData() {
    this.http.get<StorageLocation[]>(`${environment.apiBaseUrl}/locations`).subscribe({
      next: (data) => {
        this.locations = data;
        this.isLoading = false;
      },
      error: (err) => {
      console.error('СЕРВЕР СЛОМАЛСЯ:', err);
      this.isLoading = false;
    }
    });
  }

  drop(event: CdkDragDrop<Item[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const item = event.previousContainer.data[event.previousIndex];
      const targetLocationId = event.container.id;

      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );

      this.http.patch(`${environment.apiBaseUrl}/items/move`, { 
        itemId: item.id, 
        targetLocationId: targetLocationId 
      }).subscribe();
    }
  }
}