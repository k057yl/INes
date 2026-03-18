import { Component, Input, Output, EventEmitter, inject, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';

import { Item } from '../../../models/entities/item.entity';
import { FeatureService } from '../../../core/services/feature.service';
import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { environment } from '../../../../environments/environment';
import { ConfirmModalComponent } from '../../../shared/components/entity-modal/confirm-modal.component';

@Component({
  selector: 'app-item-card',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    DragDropModule, 
    TranslateModule, 
    ConfirmModalComponent
  ],
  templateUrl: './item-card.component.html',
  styleUrl: './item-card.component.scss'
})
export class ItemCardComponent {
  private el = inject(ElementRef);
  public featureService = inject(FeatureService);
  private readonly baseUrl = environment.apiBaseUrl.replace('/api', '');

  @Input({ required: true }) item!: Item;
  @Input() flatLocations: StorageLocation[] = [];
  
  @Output() sell = new EventEmitter<Item>();
  @Output() delete = new EventEmitter<Item>();
  @Output() move = new EventEmitter<{item: Item, targetLocationId: string}>();

  showMenu = false;
  showDeleteConfirm = false;
  isMobile = window.innerWidth <= 768;

  private readonly googleColors = ['var(--g-blue)', 'var(--g-red)', 'var(--g-yellow)', 'var(--g-green)'];

  getAccentColor(): string {
    const sum = this.item.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return this.googleColors[sum % this.googleColors.length];
  }

  getPhotoUrl(path: string | null | undefined): string {
    if (!path) return '';
    return path.startsWith('http') ? path : `${this.baseUrl}/${path}`;
  }

  get availableLocations(): StorageLocation[] {
    return this.flatLocations.filter(loc => loc.id !== this.item.storageLocationId);
  }

  @HostListener('window:resize')
  onResize() { this.isMobile = window.innerWidth <= 768; }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.showMenu) return;
    if (!this.el.nativeElement.contains(event.target)) this.showMenu = false;
  }

  toggleMenu(event: MouseEvent) {
    event.stopPropagation();
    this.showMenu = !this.showMenu;
  }

  onMove(event: Event) {
    const targetId = (event.target as HTMLSelectElement).value;
    if (targetId) {
      this.move.emit({ item: this.item, targetLocationId: targetId });
      this.showMenu = false;
    }
  }

  // --- ЛОГИКА УДАЛЕНИЯ ---

  requestDelete() {
    this.showMenu = false;
    this.showDeleteConfirm = true;
  }

  confirmDelete() {
    this.delete.emit(this.item);
    this.showDeleteConfirm = false;
  }

  cancelDelete() {
    this.showDeleteConfirm = false;
  }

  stopProp(event: MouseEvent) { event.stopPropagation(); }
}