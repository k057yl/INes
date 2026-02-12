import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LocationService } from '../../services/location.service';

@Component({
  selector: 'app-location-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container">
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <h2>Создать локацию</h2>
        <input formControlName="name" placeholder="Название">
        <input formControlName="color" type="color">
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
export class LocationCreateComponent {
  private fb = inject(FormBuilder);
  private locationService = inject(LocationService);
  private router = inject(Router);

  form = this.fb.group({
    name: ['', Validators.required],
    color: ['#007bff'],
    sortOrder: [0]
  });

  onSubmit() {
    if (this.form.valid) {
      this.locationService.create(this.form.value).subscribe({
        next: () => this.router.navigate(['/home']),
        error: (err) => console.error('Ошибка 401 или другая:', err)
      });
    }
  }

  cancel() { this.router.navigate(['/home']); }
}