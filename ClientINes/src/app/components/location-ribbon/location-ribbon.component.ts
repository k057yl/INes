import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DragDropModule, CdkDragDrop } from '@angular/cdk/drag-drop';
import { TranslateModule } from '@ngx-translate/core';
import { StorageLocation } from '../../models/entities/storage-location.entity';

@Component({
  selector: 'app-location-ribbon',
  standalone: true,
  imports: [CommonModule, DragDropModule, TranslateModule],
  templateUrl: './location-ribbon.component.html',
  styleUrl: './location-ribbon.component.css'
})
export class LocationRibbonComponent {
  @Input() locations: StorageLocation[] = [];
  @Input() currentPage = 0;
  @Input() activeBoardIds: string[] = []; 

  @Output() reorder = new EventEmitter<CdkDragDrop<StorageLocation[]>>();
  @Output() pageChange = new EventEmitter<number>();
  @Output() jumpTo = new EventEmitter<string>();

  readonly pageSize = 15;

  get pagedLocations(): StorageLocation[] {
    const start = this.currentPage * this.pageSize;
    return this.locations.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.locations.length / this.pageSize);
  }

  isLocActiveOnBoard(locId: string): boolean {
    return this.activeBoardIds.includes(locId);
  }
}