import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs';

import { FeatureService } from '../../core/services/feature.service';
import { CategoryService } from '../../shared/services/category.service';
import { PlatformService } from '../../shared/services/platform.service';
import { EntityModalComponent } from '../../shared/components/entity-modal/entity-modal.component';
import { ConfirmModalComponent } from '../../shared/components/entity-modal/confirm-modal.component';

interface SimpleEntity {
  id: string;
  name: string;
  color?: string;
}

type SettingsTab = 'general' | 'categories' | 'platforms';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule, EntityModalComponent, ConfirmModalComponent],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  public featureService = inject(FeatureService);
  private categoryService = inject(CategoryService);
  private platformService = inject(PlatformService);

  categories: SimpleEntity[] = [];
  platforms: SimpleEntity[] = [];
  activeTab: SettingsTab = 'general';
  isLoading = false;

  // СОСТОЯНИЕ МОДАЛОК
  modalType: 'category' | 'platform' | null = null;
  modalMode: 'create' | 'edit' = 'create';
  selectedEntity: SimpleEntity | null = null;
  showEntityModal = false;
  showDeleteModal = false;

  ngOnInit() {
    this.loadAllData();
  }

  loadAllData() {
    this.isLoading = true;
    this.categoryService.getAll().subscribe(res => this.categories = res);
    this.platformService.getAll().pipe(
      finalize(() => this.isLoading = false)
    ).subscribe(res => this.platforms = res);
  }

  // --- ОБЩАЯ ЛОГИКА ОТКРЫТИЯ ---

  private openEntityModal(type: 'category' | 'platform', mode: 'create' | 'edit', entity?: SimpleEntity) {
    this.modalType = type;
    this.modalMode = mode;
    this.selectedEntity = entity || null;
    this.showEntityModal = true;
  }

  private openDeleteModal(type: 'category' | 'platform', entity: SimpleEntity) {
    this.modalType = type;
    this.selectedEntity = entity;
    this.showDeleteModal = true;
  }

  // --- ОБРАБОТЧИКИ КНОПОК (вызываются из HTML) ---

  addCategory() { this.openEntityModal('category', 'create'); }
  
  renameCategory(cat: SimpleEntity) { this.openEntityModal('category', 'edit', cat); }
  
  deleteCategory(cat: SimpleEntity) { this.openDeleteModal('category', cat); }

  addPlatform() { this.openEntityModal('platform', 'create'); }
  
  renamePlatform(plat: SimpleEntity) { this.openEntityModal('platform', 'edit', plat); }
  
  deletePlatform(plat: SimpleEntity) { this.openDeleteModal('platform', plat); }

  // --- ЛОГИКА ПОДТВЕРЖДЕНИЯ (вызывается из модалок) ---

  handleEntityConfirm(name: string) {
    if (this.modalType === 'category') {
      if (this.modalMode === 'create') {
        this.categoryService.create({ name }).subscribe(res => this.categories.push(res));
      } else if (this.selectedEntity) {
        this.categoryService.rename(this.selectedEntity.id, name).subscribe(() => this.selectedEntity!.name = name);
      }
    } else if (this.modalType === 'platform') {
      if (this.modalMode === 'create') {
        this.platformService.create({ name }).subscribe(res => this.platforms.push(res));
      } else if (this.selectedEntity) {
        this.platformService.rename(this.selectedEntity.id, name).subscribe(() => this.selectedEntity!.name = name);
      }
    }
    this.closeModals();
  }

  handleDeleteConfirm() {
    if (!this.selectedEntity) return;
    const service = this.modalType === 'category' ? this.categoryService : this.platformService;
    
    service.delete(this.selectedEntity.id).subscribe({
      next: () => {
        if (this.modalType === 'category') {
          this.categories = this.categories.filter(c => c.id !== this.selectedEntity!.id);
        } else {
          this.platforms = this.platforms.filter(p => p.id !== this.selectedEntity!.id);
        }
      }
    });
    this.closeModals();
  }

  closeModals() {
    this.showEntityModal = false;
    this.showDeleteModal = false;
    this.selectedEntity = null;
    this.modalType = null;
  }

  switchTab(tab: SettingsTab) {
    this.activeTab = tab;
  }
}