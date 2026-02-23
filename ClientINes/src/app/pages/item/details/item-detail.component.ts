import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Item } from '../../../models/entities/item.entity';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-item-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './item-detail.component.html',
  styleUrls: ['./item-detail.component.css']
})
export class ItemDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);

  item: Item | null = null;
  isLoading = true;
  activePhotoUrl: string | null = null;
  readonly baseUrl = environment.apiBaseUrl.replace('/api', '');

  getPhotoUrl(path: string | undefined): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return `${this.baseUrl}/${path}`;
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadItem(id);
    }
  }

  loadItem(id: string) {
    this.isLoading = true;
    this.http.get<Item>(`${environment.apiBaseUrl}/items/${id}`).subscribe({
      next: (data) => {
        this.item = data;

        if (data.photoUrl) {
          this.activePhotoUrl = data.photoUrl;
        } else if (data.photos && data.photos.length > 0) {
          this.activePhotoUrl = data.photos[0].filePath;
        }

        this.isLoading = false;
      },
      error: (err) => {
        console.error('Ошибка загрузки предмета:', err);
        this.isLoading = false;
      }
    });
  }

  setMainPhoto(url: string) {
    this.activePhotoUrl = url;
  }

  goBack() {
    this.router.navigate(['/main']);
  }
}