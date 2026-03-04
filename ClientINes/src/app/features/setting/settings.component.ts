import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { TranslateModule } from '@ngx-translate/core';
import { FeatureService } from '../../core/services/feature.service';
import { RouterModule } from '@angular/router';

interface SimpleEntity {
  id: string;
  name: string;
  color?: string;
}

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
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