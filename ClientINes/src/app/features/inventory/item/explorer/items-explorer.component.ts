import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { finalize } from 'rxjs';

import { ItemService } from '../../../../shared/services/item.service';
import { SalesService } from '../../../../shared/services/sales.service';
import { Item } from '../../../../models/entities/item.entity';
import { ItemFilterBarComponent } from '../filters/item-filter-bar.component';
import { StatusNamePipe } from '../../../../shared/components/pipe/status-name.pipe';
import { InestModalComponent } from '../../../../shared/components/modal/inest-modal.component';

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

  items: Item[] = [];
  isLoading = true;

  itemToDelete: any = null;
  showDeleteModal = false;

  ngOnInit() {
    this.loadData();
  }

  loadData(filters?: any) {
    this.isLoading = true;
    this.itemService.getAll(filters)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(data => {
        this.items = data;
      });
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