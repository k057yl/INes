import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';

import { FeatureService } from '../../core/services/feature.service';
import { CategoryService } from '../../core/services/category.service';
import { PlatformService } from '../../core/services/platform.service';
import { DashboardModalService } from '../../features/dashboard/dashboard.modal.service';

interface SimpleContract {
  id: string;
  name: string;
  color?: string;
}

type SettingsTab = 'general' | 'categories' | 'platforms';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  public featureService = inject(FeatureService);
  private categoryService = inject(CategoryService);
  private platformService = inject(PlatformService);
  private modalService = inject(DashboardModalService);
  private translate = inject(TranslateService);

  categories: SimpleContract[] = [];
  platforms: SimpleContract[] = [];
  activeTab: SettingsTab = 'general';
  isLoading = false;

  ngOnInit() {
    this.loadAllData();
  }

  loadAllData() {
    this.isLoading = true;
    forkJoin({
      categories: this.categoryService.getAll(),
      platforms: this.platformService.getAll()
    }).pipe(
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (res) => {
        this.categories = res.categories;
        this.platforms = res.platforms;
      }
    });
  }

  addCategory() { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.CREATE', message: '', name: ''
    }).subscribe(res => {
      if (res) {
        this.categoryService.create({ name: res }).subscribe(() => this.loadAllData());
      }
    });
  }
  
  renameCategory(cat: SimpleContract) { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.EDIT', message: '', name: cat.name
    }).subscribe(res => {
      if (res) {
        this.categoryService.rename(cat.id, res).subscribe(() => this.loadAllData());
      }
    });
  }
  
  deleteCategory(cat: SimpleContract) { 
    this.modalService.openConfirm({
      mode: 'delete', 
      title: 'COMMON.DELETE', 
      message: this.translate.instant('SETTINGS_PAGE.MODAL.DELETE_CATEGORY'),
      name: cat.name
    }).subscribe(res => {
      if (res) {
        this.categoryService.delete(cat.id).subscribe(() => this.loadAllData());
      }
    });
  }

  addPlatform() { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.CREATE', message: '', name: ''
    }).subscribe(res => {
      if (res) {
        this.platformService.create({ name: res }).subscribe(() => this.loadAllData());
      }
    });
  }
  
  renamePlatform(plat: SimpleContract) { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.EDIT', message: '', name: plat.name
    }).subscribe(res => {
      if (res) {
        this.platformService.rename(plat.id, res).subscribe(() => this.loadAllData());
      }
    });
  }
  
  deletePlatform(plat: SimpleContract) { 
    this.modalService.openConfirm({
      mode: 'delete',
      title: 'COMMON.DELETE',
      message: this.translate.instant('SETTINGS_PAGE.MODAL.DELETE_PLATFORM'),
      name: plat.name
    }).subscribe(res => {
      if (res) {
        this.platformService.delete(plat.id).subscribe(() => this.loadAllData());
      }
    });
  }

  switchTab(tab: SettingsTab) {
    this.activeTab = tab;
  }
}