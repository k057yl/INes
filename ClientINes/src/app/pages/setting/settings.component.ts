import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TranslateModule } from '@ngx-translate/core';
import { FeatureService } from '../../services/feature.service';

interface SimpleEntity {
  id: string;
  name: string;
  color?: string;
}

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="settings-container">
      <h1><i class="fa fa-cogs"></i> Управление данными</h1>

      <div class="settings-grid">
        <section class="settings-section mode-toggle">
          <h2><i class="fa fa-sliders-h"></i> Режим работы</h2>
          
          <div class="toggle-item">
            <div class="toggle-control">
              <span>Модуль продаж</span>
              <label class="switch">
                <input type="checkbox" 
                      [checked]="featureService.isSalesModeEnabled()"
                      (change)="featureService.toggleSalesMode(s.checked)" #s>
                <span class="slider"></span>
              </label>
            </div>
            <p class="hint">Включает кнопки «Продать», выбор платформ и статистику прибыли.</p>
          </div>

          <div class="divider"></div>

          <div class="toggle-item">
            <div class="toggle-control">
              <span>Модуль одалживания</span>
              <label class="switch">
                <input type="checkbox" 
                      [checked]="featureService.isLendingModeEnabled()"
                      (change)="featureService.toggleLendingMode(l.checked)" #l>
                <span class="slider"></span>
              </label>
            </div>
            <p class="hint">Позволяет помечать предметы как переданные другим людям и создавать зоны одалживания.</p>
          </div>
        </section>

        <section class="settings-section" *ngIf="featureService.isSalesModeEnabled()">
          <div class="sec-header">
            <h2><i class="fa fa-shopping-cart"></i> Платформы</h2>
          </div>
          <div class="list">
            <div class="list-item" *ngFor="let p of platforms">
              <span>{{ p.name }}</span>
              <button class="delete-icon-btn" (click)="deletePlatform(p.id)" title="Удалить">
                <i class="fa fa-trash-alt"></i>
              </button>
            </div>
            <div *ngIf="platforms.length === 0" class="empty">Список пуст</div>
          </div>
        </section>

        <section class="settings-section">
          <div class="sec-header">
            <h2><i class="fa fa-tags"></i> Категории</h2>
          </div>
          <div class="list">
            <div class="list-item" *ngFor="let c of categories">
              <span [style.border-left]="'4px solid ' + (c.color || '#00f5d4')" class="cat-name">
                {{ c.name }}
              </span>
              <button class="delete-icon-btn" (click)="deleteCategory(c.id)" title="Удалить">
                <i class="fa fa-trash-alt"></i>
              </button>
            </div>
            <div *ngIf="categories.length === 0" class="empty">Список пуст</div>
          </div>
        </section>
      </div>
    </div>
  `,
  styles: [`
    .settings-container { padding: 40px; color: white; background: #0b132b; min-height: 100vh; }
    .settings-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 30px; }
    .settings-section { background: #1c2541; padding: 25px; border-radius: 16px; border: 1px solid #3a506b; box-shadow: 0 10px 30px rgba(0,0,0,0.2); }
    h1 { color: white; margin-bottom: 40px; font-weight: 800; display: flex; align-items: center; gap: 15px; }
    h2 { font-size: 1.1rem; color: #00f5d4; margin-bottom: 20px; text-transform: uppercase; letter-spacing: 1px; }
    
    /* Стили для переключателей */
    .toggle-item { margin-bottom: 20px; }
    .toggle-control { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
    .toggle-control span { font-weight: 600; font-size: 1rem; color: #e2e8f0; }
    .hint { color: #64748b; font-size: 0.85rem; line-height: 1.4; margin: 0; }
    .divider { height: 1px; background: #3a506b; margin: 20px 0; opacity: 0.5; }

    /* Красивый Switch */
    .switch { position: relative; display: inline-block; width: 44px; height: 22px; }
    .switch input { opacity: 0; width: 0; height: 0; }
    .slider { position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; background-color: #0b132b; transition: .4s; border-radius: 34px; border: 1px solid #3a506b; }
    .slider:before { position: absolute; content: ""; height: 14px; width: 14px; left: 3px; bottom: 3px; background-color: #94a3b8; transition: .4s; border-radius: 50%; }
    input:checked + .slider { background-color: rgba(0, 245, 212, 0.2); border-color: #00f5d4; }
    input:checked + .slider:before { transform: translateX(22px); background-color: #00f5d4; }

    .list { display: flex; flex-direction: column; gap: 10px; }
    .list-item { 
      display: flex; justify-content: space-between; align-items: center; 
      padding: 14px 18px; background: #0b132b; border-radius: 10px; border: 1px solid rgba(58, 80, 107, 0.5);
      transition: all 0.2s;
    }
    .list-item:hover { border-color: #00f5d4; background: rgba(0, 245, 212, 0.02); }
    .cat-name { padding-left: 12px; }
    .delete-icon-btn { background: transparent; border: none; color: #ff4d4d; cursor: pointer; font-size: 1.1rem; padding: 5px; transition: 0.2s; }
    .delete-icon-btn:hover { color: #ff0000; transform: scale(1.2); }
    .empty { color: #556080; font-style: italic; text-align: center; padding: 20px; }
  `]
})
export class SettingsComponent implements OnInit {
  private http = inject(HttpClient);

  public featureService = inject(FeatureService);
  
  platforms: SimpleEntity[] = [];
  categories: SimpleEntity[] = [];

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.http.get<SimpleEntity[]>(`${environment.apiBaseUrl}/platforms`).subscribe(res => this.platforms = res);
    this.http.get<SimpleEntity[]>(`${environment.apiBaseUrl}/categories`).subscribe(res => this.categories = res);
  }

  deletePlatform(id: string) {
    if (confirm('Удалить эту платформу? Это не удалит историю продаж, но связь может пропасть.')) {
      this.http.delete(`${environment.apiBaseUrl}/platforms/${id}`).subscribe({
        next: () => this.platforms = this.platforms.filter(p => p.id !== id),
        error: (err: any) => alert('Ошибка удаления: ' + (err.error?.message || 'Сервер отклонил запрос'))
      });
    }
  }

  deleteCategory(id: string) {
    if (confirm('Удалить категорию? Убедитесь, что в ней нет активных предметов.')) {
      this.http.delete(`${environment.apiBaseUrl}/categories/${id}`).subscribe({
        next: () => this.categories = this.categories.filter(c => c.id !== id),
        error: (err: any) => alert(err.error || 'Ошибка удаления. Возможно, категория не пуста.')
      });
    }
  }
}