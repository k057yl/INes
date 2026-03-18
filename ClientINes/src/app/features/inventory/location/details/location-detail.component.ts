import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, Location as NgLocation } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { switchMap } from 'rxjs/operators';
import { TranslateModule } from '@ngx-translate/core';
import { StorageLocation } from '../../../../models/entities/storage-location.entity';
import { ItemStatus } from '../../../../models/enums/item-status.enum';

@Component({
  selector: 'app-location-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './location-detail.component.html',
  styleUrl: './location-detail.component.scss'
})
export class LocationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);
  private ngLocation = inject(NgLocation);
  
  private readonly baseUrl = environment.apiBaseUrl.replace('/api', '');
  private readonly googleColors = ['var(--g-blue)', 'var(--g-red)', 'var(--g-yellow)', 'var(--g-green)'];

  location: StorageLocation | null = null;
  isLoading = true;

  readonly statusKeys: Record<number, string> = {
    [ItemStatus.Active]: 'STATUS.ACTIVE',
    [ItemStatus.Lent]: 'STATUS.LENT',
    [ItemStatus.Lost]: 'STATUS.LOST',
    [ItemStatus.Broken]: 'STATUS.BROKEN',
    [ItemStatus.Sold]: 'STATUS.SOLD',
    [ItemStatus.Gifted]: 'STATUS.GIFTED',
    [ItemStatus.Listed]: 'STATUS.LISTED'
  };

  ngOnInit() {
    this.route.params.pipe(
      switchMap(params => {
        this.isLoading = true;
        return this.http.get<StorageLocation>(`${environment.apiBaseUrl}/locations/${params['id']}`);
      })
    ).subscribe({
      next: (data) => {
        this.location = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  // --- Логика цветов и фото ---

  getAccentColor(id: string): string {
    const sum = id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return this.googleColors[sum % this.googleColors.length];
  }

  getPhotoUrl(path: string | null | undefined): string {
    if (!path) return '';
    return path.startsWith('http') ? path : `${this.baseUrl}/${path}`;
  }

  // --- Действия ---

  deleteItem(item: any) {
    // В идеале тут вызвать красивую модалку, но для начала — confirm
    if (confirm(`Удалить "${item.name}"?`)) {
      this.http.delete(`${environment.apiBaseUrl}/items/${item.id}`).subscribe({
        next: () => {
          if (this.location) {
            this.location.items = this.location.items.filter(i => i.id !== item.id);
          }
        }
      });
    }
  }

  // --- Хелперы ---

  getStatusKey(status: number): string {
    return this.statusKeys[status] || 'STATUS.ACTIVE';
  }

  getStatusClass(status: number): string {
    return ItemStatus[status]?.toLowerCase() || 'active';
  }

  goBack(): void {
    this.ngLocation.back();
  }
}