import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { Item } from '../../../../models/entities/item.entity';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-item-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './item-detail.component.html',
  styleUrls: ['./item-detail.component.scss']
})
export class ItemDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);

  item: Item | null = null;
  isLoading = true;
  activePhotoUrl: string | null = null;
  readonly baseUrl = environment.apiBaseUrl.replace('/api', '');

  private readonly googleColors = [
    'var(--g-blue)', 
    'var(--g-red)', 
    'var(--g-yellow)', 
    'var(--g-green)'
  ];

  historyIcons: { [key: number]: string } = {
    0: 'fa-plus-circle',      // Created
    1: 'fa-exchange-alt',     // Moved
    2: 'fa-info-circle',      // StatusChanged
    3: 'fa-tools',            // Repaired
    4: 'fa-hand-holding',     // Lent
    5: 'fa-undo',             // Returned
    6: 'fa-chart-line'        // ValueUpdated
  };

  statusKeys: { [key: number]: string } = {
    0: 'STATUS.ACTIVE',
    1: 'STATUS.LENT',
    2: 'STATUS.LOST',
    3: 'STATUS.BROKEN',
    4: 'STATUS.SOLD',
    5: 'STATUS.GIFTED',
    6: 'STATUS.LISTED'
  };

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadItem(id);
    } else {
      this.router.navigate(['/main']);
    }
  }

  getAccentColor(): string {
    if (!this.item) return this.googleColors[0];
    const sum = this.item.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return this.googleColors[sum % this.googleColors.length];
  }

  loadItem(id: string) {
    this.isLoading = true;
    this.http.get<Item>(`${environment.apiBaseUrl}/items/${id}`).subscribe({
      next: (data) => {
        this.item = data;
        this.activePhotoUrl = data.photoUrl || (data.photos?.length ? data.photos[0].filePath : null);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Ошибка загрузки предмета:', err);
        this.isLoading = false;

        if (err.status === 404) {
          this.router.navigate(['/main'], { replaceUrl: true });
        }
      }
    });
  }

  getPhotoUrl(path: string | null | undefined): string {
    if (!path) return 'assets/images/no-image.png';
    if (path.startsWith('http')) return path;
    return `${this.baseUrl}/${path}`;
  }

  setMainPhoto(path: string) {
    this.activePhotoUrl = path;
  }

  goBack() {
    if (window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigate(['/main']);
    }
  }
}