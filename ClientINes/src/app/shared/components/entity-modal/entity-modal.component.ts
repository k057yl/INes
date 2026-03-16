import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-entity-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
    <div class="modal-backdrop" (click)="close()">
      <div class="modal-card" (click)="$event.stopPropagation()">
        <h3>{{ title | translate }}</h3>
        <div class="inest-form-group">
          <input [(ngModel)]="name" 
                 class="inest-input-styled"
                 [placeholder]="'AUTH.FIELD_USERNAME' | translate" 
                 (keyup.enter)="submit()">
        </div>
        <div class="modal-actions" style="display: flex; gap: 10px; margin-top: 20px;">
          <button class="inest-btn-cancel" style="flex: 1" (click)="close()">
            {{ 'CREATE_ITEM.BUTTON_CANCEL' | translate }}
          </button>
          <button class="inest-btn-primary" style="flex: 1" (click)="submit()" [disabled]="!name.trim()">
            {{ 'MAIN.ADD' | translate }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-backdrop {
      position: fixed; top: 0; left: 0; width: 100%; height: 100%;
      background: rgba(0, 0, 0, 0.4);
      display: flex; align-items: center; justify-content: center; 
      z-index: 2000; backdrop-filter: blur(8px);
    }
    .modal-card {
      background: var(--bg-card); 
      padding: 24px; border-radius: 12px;
      width: 100%; max-width: 400px; 
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
      border: 1px solid var(--border-color);
    }
    h3 { margin: 0 0 20px 0; color: var(--text-main); font-weight: 600; }
    
    .inest-input-styled {
      width: 100%;
      padding: 12px 16px;
      background: var(--bg-input);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      color: var(--text-main);
      font-size: 1rem;
      transition: all 0.2s ease;
      outline: none;
      box-sizing: border-box;
    }

    .inest-input-styled:focus {
      border-color: var(--accent-color);
      box-shadow: 0 0 0 2px var(--bg-main); /* Эффект свечения через подложку */
    }

    .modal-actions { display: flex; gap: 12px; margin-top: 24px; }
    
    .modal-actions button { 
      flex: 1; padding: 10px; border-radius: 8px; 
      font-weight: 600; cursor: pointer; border: none;
      transition: all 0.2s ease;
    }

    .inest-btn-cancel { 
      background: var(--bg-cancel-button); 
      color: var(--text-main);
      border: 1px solid var(--border-color) !important;
    }
    
    .inest-btn-primary { 
      background: var(--accent-color); 
      color: var(--text-on-accent); 
    }

    .modal-actions button:hover:not(:disabled) { opacity: 0.9; transform: translateY(-1px); }
    .modal-actions button:disabled { opacity: 0.5; cursor: not-allowed; }
  `]
})
export class EntityModalComponent {
  @Input() title: string = '';
  @Input() name: string = '';
  @Output() confirmed = new EventEmitter<string>();
  @Output() cancelled = new EventEmitter<void>();

  submit() {
    if (this.name.trim()) {
      this.confirmed.emit(this.name.trim());
    }
  }

  close() {
    this.cancelled.emit();
  }
}