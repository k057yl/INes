import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CategoryService } from '../../services/category.service';
import { CreateCategoryDto } from '../../models/dtos/category.dto';

@Component({
  selector: 'app-category-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container">
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <h2>Создать категорию</h2>

        <input formControlName="name" placeholder="Название категории" required>

        <div class="actions">
          <button type="button" (click)="cancel()">Отмена</button>
          <button type="submit" [disabled]="form.invalid">Сохранить</button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .container { padding: 20px; max-width: 400px; margin: auto; }
    input { display: block; width: 100%; margin-bottom: 10px; padding: 8px; }
    .actions { display: flex; gap: 10px; }
  `]
})
export class CategoryCreateComponent {
  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  form = this.fb.group({
    name: ['', Validators.required]
  });

  onSubmit() {
    if (this.form.valid) {
      const val = this.form.value as { name: string };
      const dto: CreateCategoryDto = { name: val.name };
      this.categoryService.create(dto).subscribe({
        next: () => this.router.navigate(['/home']),
        error: err => console.error('Ошибка при создании категории', err)
      });
    }
  }

  cancel() { this.router.navigate(['/home']); }
}