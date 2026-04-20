import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { finalize } from 'rxjs';

import { ItemService } from '../../../../shared/services/item.service';
import { Item } from '../../../../models/entities/item.entity';
import { ItemFilterBarComponent } from '../filters/item-filter-bar.component';
import { StatusNamePipe } from '../../../../shared/pipe/status-name.pipe';
import { DashboardModalService } from '../../../dashboard/dashboard.modal.service';

@Component({
  selector: 'app-items-explorer',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    TranslateModule, 
    ItemFilterBarComponent, 
    StatusNamePipe
  ],
  templateUrl: './items-explorer.component.html',
  styleUrl: './items-explorer.component.scss'
})
export class ItemsExplorerComponent implements OnInit {
  private itemService = inject(ItemService);
  private modalService = inject(DashboardModalService);

  @ViewChild(ItemFilterBarComponent) filterBar!: ItemFilterBarComponent;

  items: Item[] = [];
  isLoading = true;
  currentFilters: any = {};

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

  onEditClick(item: Item) {
    this.modalService.openItemForm(item).subscribe(res => {
      if (res) this.loadData(this.currentFilters);
    });
  }

  onDeleteClick(item: Item) {
    this.modalService.openConfirm({
      mode: 'delete',
      title: 'COMMON.DELETE',
      message: 'COMMON.M_YOU_SURE'
    }).subscribe(res => {
      if (res) {
        this.isLoading = true;
        this.itemService.delete(item.id).subscribe({
          next: () => this.loadData(this.currentFilters),
          error: () => this.isLoading = false
        });
      }
    });
  }

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
}