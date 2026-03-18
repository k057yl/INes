import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="modal-backdrop" (click)="close()">
      <div class="modal-card is-danger" (click)="$event.stopPropagation()">
        <div class="warning-icon"><i class="fa fa-exclamation-triangle"></i></div>
        <h3>{{ title | translate }}</h3>
        <p class="message">{{ message | translate }}</p>
        
        <div class="modal-actions">
          <button class="inest-btn-cancel" (click)="close()">
            {{ 'COMMON.CANCEL' | translate }}
          </button>
          <button class="inest-btn-danger" (click)="confirm()">
            {{ 'ITEM_CARD.DELETE' | translate }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-backdrop {
      position: fixed; top: 0; left: 0; width: 100%; height: 100%;
      background: rgba(11, 19, 43, 0.85); display: flex; align-items: center; justify-content: center; 
      z-index: 2000; backdrop-filter: blur(8px);
    }
    .modal-card {
      background: var(--bg-card); padding: 32px; border-radius: 20px;
      width: 100%; max-width: 380px; text-align: center;
      border: 1px solid var(--border-color);
      box-shadow: 0 0 30px rgba(0, 0, 0, 0.5);
    }
    .warning-icon { 
      font-size: 3rem; color: var(--error-red); margin-bottom: 16px; 
      text-shadow: 0 0 15px rgba(255, 77, 77, 0.3);
    }
    h3 { margin: 0 0 12px 0; color: var(--text-main); font-size: 1.4rem; }
    .message { color: var(--text-muted); font-size: 0.95rem; line-height: 1.5; margin-bottom: 24px; }
    .modal-actions { display: flex; gap: 12px; }
    button { flex: 1; padding: 12px; border-radius: 10px; font-weight: 700; cursor: pointer; border: none; transition: 0.2s; }
    
    .inest-btn-cancel { background: var(--bg-input); color: var(--text-main); border: 1px solid var(--border-color); }
    .inest-btn-danger { 
      background: var(--error-red); color: #000; 
      &:hover { box-shadow: 0 0 15px rgba(255, 77, 77, 0.4); transform: translateY(-1px); }
    }
  `]
})
export class ConfirmModalComponent {
  @Input() title: string = '';
  @Input() message: string = '';
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  confirm() { this.confirmed.emit(); }
  close() { this.cancelled.emit(); }
}