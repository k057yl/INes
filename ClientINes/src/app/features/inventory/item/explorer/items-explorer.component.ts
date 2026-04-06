import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { finalize } from 'rxjs';

import { ItemService } from '../../../../shared/services/item.service';
import { SalesService } from '../../../../shared/services/sales.service';
import { Item } from '../../../../models/entities/item.entity';
import { ItemFilterBarComponent } from '../filters/item-filter-bar.component';
import { StatusNamePipe } from '../../../../shared/pipe/status-name.pipe';
import { InestModalComponent } from '../../../../shared/components/modal/shared-modal/inest-modal.component';

@Component({
  selector: 'app-items-explorer',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    TranslateModule, 
    ItemFilterBarComponent, 
    StatusNamePipe,
    InestModalComponent
  ],
  templateUrl: './items-explorer.component.html',
  styleUrl: './items-explorer.component.scss'
})
export class ItemsExplorerComponent implements OnInit {
  private itemService = inject(ItemService);
  private salesService = inject(SalesService);

  @ViewChild(ItemFilterBarComponent) filterBar!: ItemFilterBarComponent;

  items: Item[] = [];
  isLoading = true;
  currentFilters: any = {};

  itemToDelete: any = null;
  showDeleteModal = false;

  trackById = (index: number, item: any) => item.id;

  ngOnInit() {
    this.loadData();
  }

  loadData(filters?: any) {
    this.isLoading = true;
    this.currentFilters = filters || {};
    this.itemService.getAll(this.currentFilters)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(data => {
        this.items = data;
      });
  }

  // --- ЛОГИКА СОРТИРОВКИ ---

  toggleSort(asc: number, desc: number) {
    const currentSort = this.currentFilters.sortBy;
    const nextSort = currentSort === asc ? desc : asc;
    
    if (this.filterBar) {
      this.filterBar.filterForm.patchValue({ sortBy: nextSort });
    }
  }

  getSortIcon(asc: number, desc: number): string {
    const s = this.currentFilters.sortBy;
    if (s === asc) return 'fa-sort-amount-up active-sort';
    if (s === desc) return 'fa-sort-amount-down active-sort';
    return 'fa-sort muted-sort';
  }

  onDeleteClick(item: any) {
    this.itemToDelete = item;
    this.showDeleteModal = true;
  }

  closeModal() {
    this.itemToDelete = null;
    this.showDeleteModal = false;
  }

  handleDelete(eventData?: string) {
    if (!this.itemToDelete) return;

    this.isLoading = true;
    const isSold = this.itemToDelete.status === 4;
    const keepHistory = eventData === 'keep';
    const deleteObs = (isSold && this.itemToDelete.sale?.id)
      ? this.salesService.smartDelete(this.itemToDelete.sale.id, keepHistory)
      : this.itemService.delete(this.itemToDelete.id);

    deleteObs.subscribe({
      next: () => {
        this.closeModal();
        this.loadData(); 
      },
      error: () => {
        this.isLoading = false;
        this.closeModal();
      }
    });
  }
}