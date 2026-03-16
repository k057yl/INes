import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { CategoryService } from '../../shared/services/category.service';
import { PlatformService } from '../../shared/services/platform.service';
import { Category } from '../../models/entities/category.entity';
import { TranslateService } from '@ngx-translate/core';
import { EntityModalComponent } from '../../shared/components/entity-modal/entity-modal.component';

interface SimpleEntity {
  id: string;
  name: string;
}

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [CommonModule, TranslateModule, EntityModalComponent],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.scss'
})
export class AdminPanelComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private platformService = inject(PlatformService);
  private translate = inject(TranslateService);

  platforms: SimpleEntity[] = [];
  categories: Category[] = [];

  showModal = false;
  modalTitle = '';
  modalValue = '';
  modalMode: 'category' | 'platform' = 'category';
  modalAction: 'add' | 'rename' = 'add';
  selectedId: string | null = null;

  ngOnInit() {
    this.loadAllData();
  }

  loadAllData() {
    this.categoryService.getAll().subscribe(res => this.categories = res);
    this.platformService.getAll().subscribe(res => this.platforms = res);
  }

  addCategory() { this.openModal('category', 'add', 'MANAGEMENT.TITLE_CATEGORY'); }
  addPlatform() { this.openModal('platform', 'add', 'MANAGEMENT.TITLE_PLATFORM'); }
  
  renameCategory(cat: any) { this.openModal('category', 'rename', 'SHARED.TOOLTIP_RENAME', cat); }
  renamePlatform(p: any) { this.openModal('platform', 'rename', 'SHARED.TOOLTIP_RENAME', p); }

  private openModal(mode: 'category' | 'platform', action: 'add' | 'rename', title: string, entity?: any) {
    this.modalMode = mode;
    this.modalAction = action;
    this.modalTitle = title;
    this.modalValue = entity ? entity.name : '';
    this.selectedId = entity ? entity.id : null;
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.selectedId = null;
    this.modalValue = '';
  }

  onModalConfirmed(name: string) {
    const service = this.modalMode === 'category' ? this.categoryService : this.platformService;
    
    const request$ = this.modalAction === 'add' 
      ? service.create({ name }) 
      : service.update(this.selectedId!, { name });

    request$.subscribe({
      next: () => {
        this.loadAllData();
        this.closeModal();
      },
      error: (err) => {
        alert(this.translate.instant(err.error?.error || 'SYSTEM.DEFAULT_ERROR'));
        this.closeModal();
      }
    });
  }

  deleteCategory(cat: Category) {
    if (confirm(`Удалить категорию "${cat.name}"?`)) {
      this.categoryService.delete(cat.id).subscribe({
        next: () => this.loadAllData(),
        error: (err) => alert(this.translate.instant(err.error?.error || 'SYSTEM.DEFAULT_ERROR'))
      });
    }
  }

  deletePlatform(p: SimpleEntity) {
    if (confirm(`Удалить платформу "${p.name}"?`)) {
      this.platformService.delete(p.id).subscribe(() => this.loadAllData());
    }
  }
}