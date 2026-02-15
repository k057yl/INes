import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/entities/category.entity';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="category-manager">
      <div class="header">
        <h1>Управление категориями</h1>
        <button class="btn-primary" (click)="openModal()">
          <i class="fa fa-plus"></i> Новая категория
        </button>
      </div>

      <div class="category-grid">
        <div class="category-item" *ngFor="let cat of categories">
          <div class="cat-info">
            <span class="cat-name">{{ cat.name }}</span>
          </div>
          <div class="cat-actions">
            <button class="btn-icon edit" (click)="openModal(cat)" title="Переименовать">
              <i class="fa fa-edit"></i>
            </button>
            <button class="btn-icon delete" (click)="onDelete(cat)" title="Удалить">
              <i class="fa fa-trash"></i>
            </button>
          </div>
        </div>
      </div>

      <div class="modal-overlay" *ngIf="showModal" (click)="closeModal()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <h3>{{ editingCategory ? 'Редактировать' : 'Создать' }} категорию</h3>
          
          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <input 
              formControlName="name" 
              placeholder="Название (например: Инструменты)" 
              required 
              #nameInput>
            
            <div class="modal-footer">
              <button type="button" class="btn-cancel" (click)="closeModal()">Отмена</button>
              <button type="submit" class="btn-save" [disabled]="form.invalid">
                {{ editingCategory ? 'Обновить' : 'Создать' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .category-manager { padding: 40px; max-width: 800px; margin: auto; }
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; }
    
    .category-grid { display: grid; gap: 12px; }
    .category-item { 
      background: white; padding: 15px 20px; border-radius: 12px;
      display: flex; justify-content: space-between; align-items: center;
      box-shadow: 0 2px 5px rgba(0,0,0,0.05); border: 1px solid #eee;
    }
    .cat-name { font-weight: 600; color: #333; }
    
    .cat-actions { display: flex; gap: 8px; }
    .btn-icon { background: none; border: none; cursor: pointer; padding: 8px; border-radius: 6px; transition: 0.2s; }
    .btn-icon.edit { color: #007bff; background: #0777f7; }
    .btn-icon.delete { color: #dc3545; background: #ff1100; }
    .btn-icon:hover { transform: scale(1.1); }

    /* Modal Styles */
    .modal-overlay {
      position: fixed; top: 0; left: 0; width: 100%; height: 100%;
      background: rgba(0,0,0,0.5); backdrop-filter: blur(4px);
      display: flex; align-items: center; justify-content: center; z-index: 2000;
    }
    .modal-content { 
      background: white; padding: 30px; border-radius: 16px; width: 100%; max-width: 400px;
      box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1);
    }
    .modal-content h3 { margin-top: 0; margin-bottom: 20px; }
    input { width: 100%; padding: 12px; border: 1px solid #ddd; border-radius: 8px; margin-bottom: 20px; outline: none; }
    input:focus { border-color: #007bff; }
    .modal-footer { display: flex; justify-content: flex-end; gap: 10px; }
    
    .btn-primary, .btn-save { background: #007bff; color: white; border: none; padding: 10px 20px; border-radius: 8px; cursor: pointer; }
    .btn-cancel { background: #f8f9fa; border: 1px solid #ddd; padding: 10px 20px; border-radius: 8px; cursor: pointer; }
  `]
})
export class CategoryListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  
  categories: Category[] = [];
  showModal = false;
  editingCategory: Category | null = null;

  form = this.fb.group({
    name: ['', Validators.required]
  });

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.categoryService.getAll().subscribe(res => this.categories = res);
  }

  openModal(category?: Category) {
    this.editingCategory = category || null;
    this.form.reset({ name: category?.name || '' });
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.editingCategory = null;
  }

  onSubmit() {
    if (this.form.invalid) return;
    const name = this.form.value.name as string;

    if (this.editingCategory) {
      this.categoryService.update(this.editingCategory.id, { name }).subscribe(() => {
        this.loadCategories();
        this.closeModal();
      });
    } else {
      this.categoryService.create({ name }).subscribe(() => {
        this.loadCategories();
        this.closeModal();
      });
    }
  }

  onDelete(category: Category) {
    if (confirm(`Удалить категорию "${category.name}"? Это может повлиять на связанные предметы.`)) {
      this.categoryService.delete(category.id).subscribe(() => this.loadCategories());
    }
  }
}