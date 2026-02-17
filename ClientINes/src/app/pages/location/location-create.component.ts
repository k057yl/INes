import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LocationService } from '../../services/location.service';
import { FeatureService } from '../../services/feature.service';

@Component({
  selector: 'app-location-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container">
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <h2>Создать локацию</h2>
        
        <div class="form-group">
          <label>Название</label>
          <input formControlName="name" placeholder="Например: Витрина Avito или Гараж">
        </div>

        <div class="form-group">
          <label>Цвет метки</label>
          <input formControlName="color" type="color">
        </div>

        <div class="smart-options" *ngIf="featureService.isSalesModeEnabled() || featureService.isLendingModeEnabled()">
          <p class="section-label">Тип локации</p>
          
          <div class="checkbox-group" *ngIf="featureService.isSalesModeEnabled()">
            <label class="checkbox-wrapper">
              <input type="checkbox" formControlName="isSalesLocation">
              <span class="custom-check"></span>
              <span class="label-text"><i class="fa fa-shopping-cart"></i> Торговая точка</span>
            </label>
            <small>Айтемы здесь получат статус <b>Listed</b></small>
          </div>

          <div class="checkbox-group" *ngIf="featureService.isLendingModeEnabled()">
            <label class="checkbox-wrapper">
              <input type="checkbox" formControlName="isLendingLocation">
              <span class="custom-check"></span>
              <span class="label-text"><i class="fa fa-handshake"></i> Зона одалживания</span>
            </label>
            <small>Айтемы здесь получат статус <b>Lent</b></small>
          </div>
        </div>

        <div class="actions">
           <button type="button" class="btn-cancel" (click)="cancel()">Отмена</button>
           <button type="submit" class="btn-save" [disabled]="form.invalid">Сохранить</button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .container { 
      padding: 40px; 
      max-width: 450px; 
      margin: 50px auto; 
      background: #1c2541; 
      border-radius: 16px; 
      border: 1px solid #3a506b;
      color: white;
    }
    h2 { margin-top: 0; color: #00f5d4; font-weight: 800; }
    .form-group { margin-bottom: 20px; }
    label { display: block; margin-bottom: 8px; font-size: 0.9rem; color: #94a3b8; }
    input[type="text"], input:not([type="checkbox"]):not([type="color"]) { 
      display: block; width: 100%; padding: 12px; 
      background: #0b132b; border: 1px solid #3a506b; 
      color: white; border-radius: 8px; 
    }
    input[type="color"] { width: 100%; height: 40px; border: none; cursor: pointer; background: none; }
    
    .smart-options { 
      margin: 25px 0; padding: 15px; 
      background: rgba(0, 245, 212, 0.05); 
      border-radius: 12px; border: 1px dashed #3a506b;
    }
    .section-label { font-size: 0.75rem; text-transform: uppercase; color: #5c6d8a; font-weight: bold; margin-bottom: 15px; }
    
    .checkbox-group { margin-bottom: 15px; }
    .checkbox-wrapper { display: flex; align-items: center; cursor: pointer; gap: 10px; font-weight: 600; }
    .label-text i { color: #00f5d4; margin-right: 5px; }
    small { display: block; margin-left: 26px; color: #64748b; font-size: 0.8rem; }

    .actions { display: flex; gap: 15px; margin-top: 30px; }
    button { flex: 1; padding: 12px; border-radius: 8px; cursor: pointer; font-weight: bold; transition: 0.3s; }
    .btn-cancel { background: transparent; border: 1px solid #3a506b; color: #94a3b8; }
    .btn-save { background: #00f5d4; border: none; color: #0b132b; }
    .btn-save:disabled { background: #3a506b; color: #1c2541; }
  `]
})
export class LocationCreateComponent {
  private fb = inject(FormBuilder);
  private locationService = inject(LocationService);
  private router = inject(Router);
  public featureService = inject(FeatureService);

  form = this.fb.group({
    name: ['', Validators.required],
    color: ['#007bff'],
    sortOrder: [0],
    isSalesLocation: [false],
    isLendingLocation: [false]
  });

  onSubmit() {
    if (this.form.valid) {
      this.locationService.create(this.form.value).subscribe({
        next: () => this.router.navigate(['/main']),
        error: (err) => console.error('Ошибка сохранения:', err)
      });
    }
  }

  cancel() { this.router.navigate(['/main']); }
}