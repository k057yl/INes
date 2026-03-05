import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { CategoryService } from '../../shared/services/category.service';
import { PlatformService } from '../../shared/services/platform.service';
import { Category } from '../../models/entities/category.entity';

interface SimpleEntity {
  id: string;
  name: string;
}

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.scss'
})
export class AdminPanelComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private platformService = inject(PlatformService);

  platforms: SimpleEntity[] = [];
  categories: Category[] = [];

  ngOnInit() {
    this.loadAllData();
  }

  loadAllData() {
    this.categoryService.getAll().subscribe(res => this.categories = res);
    this.platformService.getAll().subscribe(res => this.platforms = res);
  }

  // --- Категории ---
  addCategory() {
    const name = prompt('Название новой категории:');
    if (name?.trim()) {
      this.categoryService.create({ name: name.trim() }).subscribe(() => this.loadAllData());
    }
  }

  renameCategory(cat: SimpleEntity) {
    const newName = prompt('Переименовать категорию:', cat.name);
    if (newName?.trim() && newName !== cat.name) {
      this.categoryService.update(cat.id, { name: newName.trim() }).subscribe(() => this.loadAllData());
    }
  }

  deleteCategory(cat: Category) {
    if (cat.name === 'Other') {
      alert('Эту категорию нельзя удалить, она системная.');
      return;
    }

    if (confirm(`Удалить категорию "${cat.name}"? Предметы будут перенесены в "Разное".`)) {
      this.categoryService.delete(cat.id).subscribe({
        next: () => this.loadAllData(),
        error: () => alert('Ошибка удаления')
      });
    }
  }

  // --- Платформы ---
  addPlatform() {
    const name = prompt('Название платформы:');
    if (name?.trim()) {
      this.platformService.create(name.trim()).subscribe(() => this.loadAllData());
    }
  }

  renamePlatform(p: SimpleEntity) {
    const newName = prompt('Новое название:', p.name);
    if (newName?.trim() && newName !== p.name) {
      this.platformService.update(p.id, newName.trim()).subscribe(() => this.loadAllData());
    }
  }

  deletePlatform(p: SimpleEntity) {
    if (confirm(`Удалить платформу "${p.name}"?`)) {
      this.platformService.delete(p.id).subscribe(() => this.loadAllData());
    }
  }
}