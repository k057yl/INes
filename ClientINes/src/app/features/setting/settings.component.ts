import { Component, OnInit, inject } from '@angular/core';

import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs';

import { FeatureService } from '../../core/services/feature.service';
import { CategoryService } from '../../shared/services/category.service';
import { PlatformService } from '../../shared/services/platform.service';
import { DashboardModalService } from '../../features/dashboard/dashboard.modal.service';

interface SimpleEntity {
  id: string;
  name: string;
  color?: string;
}

type SettingsTab = 'general' | 'categories' | 'platforms';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [TranslateModule, RouterModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  public featureService = inject(FeatureService);
  private categoryService = inject(CategoryService);
  private platformService = inject(PlatformService);
  private modalService = inject(DashboardModalService);
  private translate = inject(TranslateService);

  categories: SimpleEntity[] = [];
  platforms: SimpleEntity[] = [];
  activeTab: SettingsTab = 'general';
  isLoading = false;

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

  addCategory() { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.CREATE', message: '', name: ''
    }).subscribe(res => {
      if (res) {
        this.categoryService.create({ name: res }).subscribe(() => this.loadAllData());
      }
    });
  }
  
  renameCategory(cat: SimpleEntity) { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.EDIT', message: '', name: cat.name
    }).subscribe(res => {
      if (res) {
        this.categoryService.rename(cat.id, res).subscribe(() => this.loadAllData());
      }
    });
  }
  
  deleteCategory(cat: SimpleEntity) { 
    this.modalService.openConfirm({
      mode: 'delete', title: 'COMMON.DELETE', message: this.translate.instant('SETTINGS_PAGE.MODAL.DELETE_CATEGORY')
    }).subscribe(res => {
      if (res) {
        this.categoryService.delete(cat.id).subscribe(() => {
          this.categories = this.categories.filter(c => c.id !== cat.id);
        });
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
  
  renamePlatform(plat: SimpleEntity) { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.EDIT', message: '', name: plat.name
    }).subscribe(res => {
      if (res) {
        this.platformService.rename(plat.id, res).subscribe(() => this.loadAllData());
      }
    });
  }
  
  deletePlatform(plat: SimpleEntity) { 
    this.modalService.openConfirm({
      mode: 'delete',
      title: 'COMMON.DELETE',
      message: this.translate.instant('SETTINGS_PAGE.MODAL.DELETE_PLATFORM')
    }).subscribe(res => {
      if (res) {
        this.platformService.delete(plat.id).subscribe(() => {
          this.platforms = this.platforms.filter(p => p.id !== plat.id);
        });
      }
    });
  }

  switchTab(tab: SettingsTab) {
    this.activeTab = tab;
  }
}