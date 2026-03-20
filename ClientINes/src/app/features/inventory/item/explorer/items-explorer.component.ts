import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { finalize, forkJoin } from 'rxjs';

import { ItemService } from '../../../../shared/services/item.service';
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

  items: Item[] = [];
  isLoading = true;

  selectedItems = new Set<string>();

  ngOnInit() {
    this.loadData();
  }

  loadData(filters?: any) {
    this.isLoading = true;
    this.itemService.getAll(filters)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(data => {
        this.items = data;
        this.selectedItems.clear();
      });
  }

  toggleSelection(itemId: string) {
    if (this.selectedItems.has(itemId)) {
      this.selectedItems.delete(itemId);
    } else {
      this.selectedItems.add(itemId);
    }
  }

  selectAll(event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      this.items.forEach(i => this.selectedItems.add(i.id));
    } else {
      this.selectedItems.clear();
    }
  }

  get isAllSelected(): boolean {
    return this.items.length > 0 && this.selectedItems.size === this.items.length;
  }

  get isIndeterminate(): boolean {
    return this.selectedItems.size > 0 && this.selectedItems.size < this.items.length;
  }

  bulkMove() {
    console.log('Moving items:', Array.from(this.selectedItems));
  }

  bulkDelete() {
    if (confirm(`Удалить выбранные предметы (${this.selectedItems.size} шт.)?`)) {
      const ids = Array.from(this.selectedItems);
      this.isLoading = true;

      const deleteRequests = ids.map(id => this.itemService.delete(id));
      
      forkJoin(deleteRequests).subscribe({
        next: () => {
          this.loadData();
        },
        error: (err) => console.error('Bulk delete failed', err)
      });
    }
  }
}