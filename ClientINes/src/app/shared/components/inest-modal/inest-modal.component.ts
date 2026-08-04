import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-inest-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './inest-modal.component.html',
  styleUrl: './inest-modal.component.scss'
})
export class InestModalComponent implements AfterViewInit {
  @Input() mode: 'input' | 'delete' | 'confirm' = 'input';
  
  @Input() title: string = '';
  @Input() message: string = '';
  @Input() name: string = '';
  @Input() placeholder: string = 'COMMON.ENTER_NAME';
  
  @Input() confirmText: string = 'COMMON.SAVE';
  @Input() cancelText: string = 'COMMON.CANCEL';

  @Output() confirmed = new EventEmitter<string>();
  @Output() cancelled = new EventEmitter<void>();

  @ViewChild('inputElement') inputElement?: ElementRef;

  ngAfterViewInit() {
    if (this.mode === 'input') {
      setTimeout(() => this.inputElement?.nativeElement.focus(), 100);
    }
  }

  getIcon(): string {
    switch (this.mode) {
      case 'delete': return 'fa-exclamation-triangle';
      case 'confirm': return 'fa-undo-alt';
      default: return 'fa-edit';
    }
  }

  getButtonClass(): string {
    switch (this.mode) {
      case 'delete': return 'inest-btn-danger';
      case 'confirm': return 'inest-btn-confirm';
      default: return 'inest-btn-primary';
    }
  }

  getConfirmText(): string {
    if (this.confirmText !== 'COMMON.SAVE') return this.confirmText;

    switch (this.mode) {
      case 'delete': return 'COMMON.DELETE';
      case 'confirm': return 'COMMON.OK';
      default: return 'COMMON.SAVE';
    }
  }

  submit(result?: string) {
    const finalValue = result || (this.mode === 'input' ? this.name.trim() : this.mode);

    if (this.mode === 'input' && !finalValue) return;

    this.confirmed.emit(finalValue);
  }

  close() {
    this.cancelled.emit();
  }
}