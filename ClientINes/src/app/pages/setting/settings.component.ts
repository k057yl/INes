import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TranslateModule } from '@ngx-translate/core';

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
        <section class="settings-section">
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