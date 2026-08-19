import { Component, Input, Output, EventEmitter, HostListener, ElementRef, inject, ViewChild } from '@angular/core';
import { DragDropModule, CdkDragDrop } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';
import { StorageLocation } from '../../../location/contracts/storage-location';
import { RouterModule } from '@angular/router';
import { RIBBON_CONFIG } from '../../../../shared/constants/ui.constants';

@Component({
  selector: 'app-location-ribbon',
  standalone: true,
  imports: [DragDropModule, TranslateModule, RouterModule],
  templateUrl: './location-ribbon.component.html',
  styleUrl: './location-ribbon.component.scss'
})
export class LocationRibbonComponent {
  private elementRef = inject(ElementRef);

  @ViewChild('createMenuContainer') createMenuContainer!: ElementRef;

  @Input() locations: StorageLocation[] = [];
  @Input() currentPage = 0;
  @Input() activeBoardIds: string[] = []; 

  @Output() reorder = new EventEmitter<CdkDragDrop<StorageLocation[]>>();
  @Output() pageChange = new EventEmitter<number>();
  @Output() jumpTo = new EventEmitter<string>();

  isCreateMenuOpen = false;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (this.isCreateMenuOpen && 
        this.createMenuContainer && 
        !this.createMenuContainer.nativeElement.contains(event.target)) {
      this.isCreateMenuOpen = false;
    }
  }

  get dynamicPageSize(): number {
    return window.innerWidth <= RIBBON_CONFIG.BREAKPOINT_MOBILE 
      ? RIBBON_CONFIG.PAGE_SIZE_MOBILE 
      : RIBBON_CONFIG.PAGE_SIZE_DESKTOP;
  }

  get pagedLocations(): StorageLocation[] {
    const start = this.currentPage * this.dynamicPageSize;
    return this.locations.slice(start, start + this.dynamicPageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.locations.length / this.dynamicPageSize));
  }

  isLocActiveOnBoard(locId: string): boolean {
    return this.activeBoardIds.includes(locId);
  }

  onLocationClick(locId: string): void {
    this.jumpTo.emit(locId);

    if (window.innerWidth <= RIBBON_CONFIG.BREAKPOINT_MOBILE) {
      setTimeout(() => {
        const targetElem = document.getElementById(`location-card-${locId}`) || 
                           document.querySelector(`[data-location-id="${locId}"]`);
        if (targetElem) {
          targetElem.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
      }, 100);
    }
  }

  toggleCreateMenu(event: MouseEvent) {
    event.stopPropagation();
    this.isCreateMenuOpen = !this.isCreateMenuOpen;
  }

  closeMenu() {
    this.isCreateMenuOpen = false;
  }
}