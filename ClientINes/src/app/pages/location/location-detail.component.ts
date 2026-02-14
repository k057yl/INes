import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-location-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="detail-container" *ngIf="location">
      <header class="detail-header">
        <div class="header-left">
          <button routerLink="/home" class="back-btn">
            <i class="fa fa-arrow-left"></i> Назад
          </button>
          <div class="title-info">
            <i [className]="'fa ' + (location.icon || 'fa-folder')" [style.color]="location.color"></i>
            <h1>{{ location.name }}</h1>
          </div>
        </div>
        <button class="add-item-btn" [routerLink]="['/create-item']" [queryParams]="{locationId: location.id}">
          <i class="fa fa-plus"></i> Добавить предмет
        </button>
      </header>

      <div class="content-layout">
        <aside class="sub-locations-panel" *ngIf="location.children?.length">
          <h3><i class="fa fa-sitemap"></i> Вложенные локации</h3>
          <ul class="sub-loc-list">
            <li *ngFor="let child of location.children">
              <a [routerLink]="['/location', child.id]" class="sub-loc-link">
                <i [className]="'fa ' + (child.icon || 'fa-folder')" [style.color]="child.color"></i>
                <span>{{ child.name }}</span>
                <i class="fa fa-chevron-right arrow"></i>
              </a>
            </li>
          </ul>
        </aside>

        <main class="items-grid-container">
          <div class="grid-header">
            <h2><i class="fa fa-box"></i> Предметы ({{ location.items?.length || 0 }})</h2>
          </div>

          <div class="items-grid" *ngIf="location.items?.length; else emptyState">
            <div class="item-tile" *ngFor="let item of location.items">
              <div class="item-image">
                <img *ngIf="item.photoUrl" [src]="item.photoUrl" loading="lazy">
                <i *ngIf="!item.photoUrl" class="fa fa-box-open placeholder"></i>
              </div>
              <div class="item-info">
                <h3>{{ item.name }}</h3>
                <p class="description">{{ item.description || 'Нет описания' }}</p>
                <div class="item-footer">
                  <span class="status-badge" [ngClass]="getStatusLabel(item.status).toLowerCase()">
                    {{ getStatusLabel(item.status) }}
                  </span>
                  <button class="edit-item-btn"><i class="fa fa-pencil"></i></button>
                </div>
              </div>
            </div>
          </div>

          <ng-template #emptyState>
            <div class="empty-view">
              <i class="fa fa-search"></i>
              <p>В этой локации пока нет предметов</p>
            </div>
          </ng-template>
        </main>
      </div>
    </div>
  `,
  styles: [`
    .detail-container { padding: 20px; max-width: 1400px; margin: 0 auto; }
    .detail-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; padding-bottom: 20px; border-bottom: 1px solid #eee; }
    .header-left { display: flex; align-items: center; gap: 20px; }
    .title-info { display: flex; align-items: center; gap: 15px; }
    .title-info h1 { margin: 0; font-size: 1.8rem; color: #333; }
    .back-btn { background: #f0f0f0; border: none; padding: 10px 15px; border-radius: 8px; cursor: pointer; transition: 0.2s; }
    .back-btn:hover { background: #e0e0e0; }

    .content-layout { display: grid; grid-template-columns: 300px 1fr; gap: 30px; }

    .sub-locations-panel { background: #f9f9f9; padding: 20px; border-radius: 12px; height: fit-content; border: 1px solid #f0f0f0; }
    .sub-locations-panel h3 { font-size: 0.9rem; color: #888; text-transform: uppercase; margin-bottom: 15px; letter-spacing: 1px; }
    .sub-loc-list { list-style: none; padding: 0; margin: 0; }
    .sub-loc-link { display: flex; align-items: center; gap: 12px; padding: 12px; text-decoration: none; color: #444; border-radius: 8px; transition: 0.2s; margin-bottom: 5px; background: white; border: 1px solid #eee; }
    .sub-loc-link:hover { background: #eef6ff; border-color: #007bff; transform: translateX(5px); }
    .sub-loc-link .arrow { margin-left: auto; font-size: 0.8rem; color: #ccc; }

    .items-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 25px; }
    .item-tile { background: white; border-radius: 15px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); border: 1px solid #eee; transition: all 0.3s ease; }
    .item-tile:hover { transform: translateY(-8px); box-shadow: 0 15px 30px rgba(0,0,0,0.1); }
    .item-image { height: 180px; background: #fcfcfc; display: flex; align-items: center; justify-content: center; overflow: hidden; border-bottom: 1px solid #f9f9f9; }
    .item-image img { width: 100%; height: 100%; object-fit: cover; }
    
    .item-info { padding: 15px; }
    .item-info h3 { margin: 0 0 8px 0; font-size: 1.15rem; color: #222; font-weight: 600; }
    .description { font-size: 0.85rem; color: #666; margin-bottom: 15px; height: 34px; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; }
    
    .item-footer { display: flex; justify-content: space-between; align-items: center; }
    .status-badge { font-size: 0.75rem; padding: 5px 12px; border-radius: 50px; background: #f0f0f0; font-weight: 500; }
    
    .status-badge.active { background: #e6ffed; color: #28a745; }
    .status-badge.lost { background: #fff5f5; color: #dc3545; }
    .status-badge.lent { background: #fff9db; color: #f08c00; }

    .add-item-btn { background: #007bff; color: white; border: none; padding: 12px 24px; border-radius: 10px; cursor: pointer; font-weight: 600; transition: 0.3s; }
    .add-item-btn:hover { background: #0056b3; box-shadow: 0 4px 12px rgba(0,123,255,0.3); }
    
    .edit-item-btn { background: none; border: 1px solid #eee; padding: 6px 10px; border-radius: 6px; cursor: pointer; color: #888; transition: 0.2s; }
    .edit-item-btn:hover { background: #f8f9fa; color: #007bff; border-color: #007bff; }

    .empty-view { text-align: center; padding: 100px 0; color: #adb5bd; }
    .empty-view i { font-size: 5rem; margin-bottom: 20px; opacity: 0.3; }
  `]
})
export class LocationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);
  
  location: any;
  isLoading = true;

  statuses = [
    { value: 0, label: 'Active' },
    { value: 1, label: 'Lent' },
    { value: 2, label: 'Lost' },
    { value: 3, label: 'Broken' },
    { value: 4, label: 'Sold' },
    { value: 5, label: 'Gifted' }
  ];

  ngOnInit() {
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) this.loadLocation(id);
    });
  }

  loadLocation(id: string) {
    this.isLoading = true;
    this.http.get(`${environment.apiBaseUrl}/locations/${id}`).subscribe({
      next: (data) => {
        this.location = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  getStatusLabel(value: number): string {
    const status = this.statuses.find(s => s.value === value);
    return status ? status.label : 'Active';
  }
}