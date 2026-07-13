import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap, throwError, of } from 'rxjs';
import { StorageLocation } from '../../core/models/entities/storage-location.entity';
import { Item } from '../../core/models/entities/item.entity';
import { LocationService } from '../../core/services/location.service';
import { ItemService } from '../../core/services/item.service';
import { SalesService } from '../../core/services/sales.service';
import { LendingService } from '../../core/services/lending.service';
import { SellItemRequestDto } from '../../core/models/dtos/sale.dto';
import { LendItemDto, ReturnItemDto } from '../../core/models/dtos/lending.dto';
import { DashboardTreeService } from './dashboard-tree.service';

@Injectable()
export class DashboardFacade {
  private locationService = inject(LocationService);
  private itemService = inject(ItemService);
  private salesService = inject(SalesService);
  private lendingService = inject(LendingService);
  private treeService = inject(DashboardTreeService);
  private router = inject(Router);

  locations: StorageLocation[] = [];
  flatLocations: StorageLocation[] = [];
  connectedLists: string[] = [];
  connectedLocationLists: string[] = [];
  isLoading = true;
  draggedLocationId: string | null = null;

  loadData(): Observable<StorageLocation[]> {
    this.isLoading = true;
    return this.locationService.getTree().pipe(
      tap({
        next: (data) => {
          this.locations = data;
          this.refreshState();
          this.isLoading = false;
        },
        error: () => (this.isLoading = false)
      })
    );
  }

  refreshState() {
    this.flatLocations = this.treeService.flattenLocations(this.locations);
    this.connectedLists = this.flatLocations.map(l => l.id);
    this.connectedLocationLists = this.flatLocations.map(l => 'list-loc-' + l.id);
  }

  deleteLocation(id: string): Observable<void> {
    return this.locationService.delete(id).pipe(
      tap(() => {
        this.locations = this.treeService.excludeLocation(this.locations, id);
        this.refreshState();
      })
    );
  }

  deleteItem(id: string): Observable<any> {
    return this.itemService.delete(id);
  }

  renameLocation(id: string, newName: string): Observable<any> {
    return this.locationService.rename(id, newName).pipe(
      tap(() => {
        const loc = this.flatLocations.find(l => l.id === id);
        if (loc) loc.name = newName;
      })
    );
  }

  moveItemLocally(item: Item, targetLocId: string) {
    const sourceLoc = this.flatLocations.find(l => l.id === item.storageLocationId);
    const targetLoc = this.flatLocations.find(l => l.id === targetLocId);

    if (sourceLoc && targetLoc) {
      sourceLoc.items = (sourceLoc.items || []).filter(i => i.id !== item.id);
      item.storageLocationId = targetLocId;
      targetLoc.items = [...(targetLoc.items || []), item];
      this.refreshState();
    }
  }

  moveLocationLocally(locId: string, targetId: string | null) {
    let movedLoc: StorageLocation | null = null;
    
    const findAndRemove = (tree: StorageLocation[], id: string): StorageLocation | null => {
      for (let i = 0; i < tree.length; i++) {
        if (tree[i].id === id) return tree.splice(i, 1)[0];
        const children = tree[i].children;
        if (children && children.length > 0) {
          const found = findAndRemove(children, id);
          if (found) return found;
        }
      }
      return null;
    };

    movedLoc = findAndRemove(this.locations, locId);
    
    if (movedLoc) {
      if (!targetId || targetId === 'root') {
        this.locations.push(movedLoc);
      } else {
        const targetLoc = this.flatLocations.find(l => l.id === targetId);
        if (targetLoc) {
          if (!targetLoc.children) targetLoc.children = [];
          targetLoc.children.push(movedLoc);
        }
      }
      this.locations = [...this.locations];
      this.refreshState();
    }
  }

  public moveLocationUpDown(locId: string, direction: 'up' | 'down') {
    const parentId = this.treeService.getParentId(this.flatLocations, locId);
    let targetArray = parentId 
      ? this.flatLocations.find(l => l.id === parentId)?.children || []
      : this.locations;

    const index = targetArray.findIndex(l => l.id === locId);
    if (index === -1) return;

    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex < 0 || newIndex >= targetArray.length) return;

    [targetArray[index], targetArray[newIndex]] = [targetArray[newIndex], targetArray[index]];
    
    this.refreshState();
    const orderedIds = targetArray.map(l => l.id);
    this.reorderLocations(orderedIds, parentId).subscribe();
  }

  moveLocationApi(locId: string, targetId: string | null): Observable<any> {
    return this.locationService.move(locId, targetId);
  }

  moveItemApi(itemId: string, targetLocId: string) {
    return this.itemService.move(itemId, targetLocId);
  }

  reorderLocations(orderedIds: string[], parentId: string | null = null) {
    return this.locationService.reorder({ parentId, orderedIds });
  }

  moveLocation(locId: string, targetId: string | null): Observable<any> {
    const normalizedTargetId = (targetId === 'root' || !targetId) ? null : targetId;
    const currentParentId = this.treeService.getParentId(this.flatLocations, locId);

    if (currentParentId === normalizedTargetId) {
      return of(null);
    }

    if (!this.treeService.canMoveLocation(this.flatLocations, locId, normalizedTargetId)) {
      return throwError(() => 'TOO_DEEP');
    }

    const previousLocations = JSON.parse(JSON.stringify(this.locations));

    try {
      this.moveLocationLocally(locId, normalizedTargetId);
    } catch (e) {
      this.locations = previousLocations;
      return throwError(() => 'LOCAL_MOVE_FAILED');
    }

    return this.locationService.move(locId, normalizedTargetId).pipe(
      tap({
        error: () => {
          this.locations = previousLocations;
          this.refreshState();
        }
      })
    );
  }

  sellItem(dto: SellItemRequestDto) {
    return this.salesService.sellItem(dto).pipe(
      tap(() => this.router.navigate(['/sales']))
    );
  }

  lendItem(dto: LendItemDto) {
    return this.lendingService.lendItem(dto);
  }
  
  updateItem(itemId: string, formData: FormData) {
    return this.itemService.update(itemId, formData);
  }

  returnItem(itemId: string, dto: ReturnItemDto = { returnedDate: new Date().toISOString() }) {
    return this.lendingService.returnItem(itemId, dto);
  }

  changeItemStatus(itemId: string, newStatus: number) {
    return this.itemService.changeStatus(itemId, newStatus).pipe(
      tap(() => this.loadData().subscribe())
    );
  }
}