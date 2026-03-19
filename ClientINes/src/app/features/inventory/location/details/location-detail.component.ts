import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, Location as NgLocation } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { switchMap } from 'rxjs/operators';

import { environment } from '../../../../../environments/environment';
import { StorageLocation } from '../../../../models/entities/storage-location.entity';
import { ItemStatus } from '../../../../models/enums/item-status.enum';
import { ItemService } from '../../../../shared/services/item.service';
import { LocationService } from '../../../../shared/services/location.service';
import { InestModalComponent } from '../../../../shared/components/modal/inest-modal.component';

@Component({
  selector: 'app-location-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, InestModalComponent],
  templateUrl: './location-detail.component.html',
  styleUrl: './location-detail.component.scss'
})
export class LocationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private itemService = inject(ItemService);
  private locationService = inject(LocationService);
  private ngLocation = inject(NgLocation);
  
  private readonly baseUrl = environment.apiBaseUrl.replace('/api', '');
  private readonly googleColors = ['var(--g-blue)', 'var(--g-red)', 'var(--g-yellow)', 'var(--g-green)'];

  location: StorageLocation | null = null;
  isLoading = true;

  // Состояние модалки удаления
  showDeleteModal = false;
  selectedItem: any = null;

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
        return this.locationService.getById(params['id']);
      })
    ).subscribe({
      next: (data) => {
        this.location = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  // --- ЛОГИКА МОДАЛКИ УДАЛЕНИЯ ---

  openDeleteModal(item: any) {
    this.selectedItem = item;
    this.showDeleteModal = true;
  }

  confirmDeleteItem() {
    if (!this.selectedItem) return;
    
    this.itemService.delete(this.selectedItem.id).subscribe({
      next: () => {
        if (this.location) {
          this.location.items = this.location.items.filter(i => i.id !== this.selectedItem.id);
        }
        this.closeModals();
      }
    });
  }

  closeModals() {
    this.showDeleteModal = false;
    this.selectedItem = null;
  }

  // --- ХЕЛПЕРЫ ---

  getAccentColor(id: string): string {
    const sum = id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return this.googleColors[sum % this.googleColors.length];
  }

  getPhotoUrl(path: string | null | undefined): string {
    if (!path) return '';
    return path.startsWith('http') ? path : `${this.baseUrl}/${path}`;
  }

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