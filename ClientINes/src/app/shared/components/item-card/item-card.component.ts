import { Component, Input, Output, EventEmitter, inject, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { Item } from '../../../models/entities/item.entity';
import { FeatureService } from '../../../core/services/feature.service';
import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { environment } from '../../../../environments/environment';
import { InestModalComponent } from '../modals/inest-modal/inest-modal.component'; 

@Component({
  selector: 'app-item-card',
  standalone: true,
  imports: [CommonModule, RouterModule, DragDropModule, TranslateModule, InestModalComponent],
  templateUrl: './item-card.component.html',
  styleUrl: './item-card.component.scss'
})
export class ItemCardComponent {
  private el = inject(ElementRef);
  public featureService = inject(FeatureService);
  private readonly baseUrl = environment.apiBaseUrl.replace('/api', '');
  private translate = inject(TranslateService);

  @Input({ required: true }) item!: Item;
  @Input() flatLocations: StorageLocation[] = [];
  @Input() accentColor?: string;
  @Input() menuOpenItemId: string | null = null;

  @Output() menuOpenedItemIdChange = new EventEmitter<string | null>();
  @Output() sell = new EventEmitter<Item>();
  @Output() delete = new EventEmitter<Item>();
  @Output() lend = new EventEmitter<Item>();
  @Output() returnItem = new EventEmitter<Item>();
  @Output() move = new EventEmitter<{item: Item, targetLocationId: string}>();
  @Output() edit = new EventEmitter<Item>();
  @Output() statusChange = new EventEmitter<{item: Item, newStatus: number}>();

  isMobile = window.innerWidth <= 768;
  showStatusModal = false;
  pendingStatus: number | null = null;
  pendingStatusNameKey = '';

  get showMenu(): boolean {
    return this.menuOpenItemId === this.item.id;
  }

  get canSell(): boolean {
    const forbiddenStatuses = [1, 7, 4];
    return !forbiddenStatuses.includes(this.item.status);
  }

  get canLend(): boolean {
    const forbiddenStatuses = [1, 7, 4];
    return !forbiddenStatuses.includes(this.item.status);
  }

  get canReturn(): boolean {
    return this.item.status === 1 || this.item.status === 7;
  }

  private readonly googleColors = ['var(--g-blue)', 'var(--g-red)', 'var(--g-yellow)', 'var(--g-green)'];

  getAccentColor(): string {
    if (this.accentColor) return this.accentColor;
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
    if (!this.el.nativeElement.contains(event.target)) {
      this.closeMenu();
    }
  }

  toggleMenu(event: MouseEvent) {
    const nextState = this.showMenu ? null : this.item.id;
    this.menuOpenedItemIdChange.emit(nextState);
  }

  closeMenu(event?: Event) {
    event?.stopPropagation();
    this.menuOpenedItemIdChange.emit(null);
  }

  onMove(event: Event) {
    const targetId = (event.target as HTMLSelectElement).value;
    if (targetId) {
      this.move.emit({ item: this.item, targetLocationId: targetId });
      this.closeMenu();
    }
  }

  openStatusModal(statusId: number, translationKey: string) {
    this.pendingStatus = statusId;
    this.pendingStatusNameKey = translationKey;
    this.showStatusModal = true;
    this.closeMenu();
  }

  confirmStatusChange() {
    if (this.pendingStatus !== null) {
      this.statusChange.emit({ item: this.item, newStatus: this.pendingStatus });
    }
    this.showStatusModal = false;
    this.pendingStatus = null;
  }

  cancelStatusModal() {
    this.showStatusModal = false;
    this.pendingStatus = null;
  }
}