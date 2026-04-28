import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap, throwError, of } from 'rxjs';
import { StorageLocation } from '../../models/entities/storage-location.entity';
import { Item } from '../../models/entities/item.entity';
import { LocationService } from '../../shared/services/location.service';
import { ItemService } from '../../shared/services/item.service';
import { SalesService } from '../../shared/services/sales.service';
import { LendingService } from '../../shared/services/lending.service';
import { SellItemRequestDto } from '../../models/dtos/sale.dto';
import { LendItemDto, ReturnItemDto } from '../../models/dtos/lending.dto';

@Injectable()
export class DashboardFacade {
  private locationService = inject(LocationService);
  private itemService = inject(ItemService);
  private salesService = inject(SalesService);
  private lendingService = inject(LendingService);
  private router = inject(Router);

  locations: StorageLocation[] = [];
  flatLocations: StorageLocation[] = [];
  connectedLists: string[] = [];
  connectedLocationLists: string[] = [];
  isLoading = true;
  draggedLocationId: string | null = null;

  private getParentId(locId: string): string | null {
    const parent = this.flatLocations.find(l => l.children?.some(c => c.id === locId));
    return parent ? parent.id : null;
  }

  public getLocationLevel(locId: string | null): number {
    if (!locId || locId === 'root') return 0;
    const loc = this.flatLocations.find(l => l.id === locId);
    if (!loc) return 0;

    const pid = this.getParentId(loc.id);
    return 1 + (pid ? this.getLocationLevel(pid) : 0);
  }

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
    this.flatLocations = this.flattenLocations(this.locations);
    this.connectedLists = this.flatLocations.map(l => l.id);
    this.connectedLocationLists = this.flatLocations.map(l => 'list-loc-' + l.id);
  }

  public flattenLocations(locs: StorageLocation[]): StorageLocation[] {
    return locs.reduce<StorageLocation[]>((acc, l) => {
      acc.push(l);
      if (l.children?.length) acc.push(...this.flattenLocations(l.children));
      return acc;
    }, []);
  }

  deleteLocation(id: string): Observable<void> {
    return this.locationService.delete(id).pipe(
      tap(() => {
        this.removeLocationFromTree(this.locations, id);
        this.locations = [...this.locations];
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
      sourceLoc.items = sourceLoc.items.filter(i => i.id !== item.id);
      item.storageLocationId = targetLocId;
      targetLoc.items.push(item);
      this.refreshState();
    }
  }

  moveLocationLocally(locId: string, targetId: string | null) {
    const movedLoc = this.findAndRemoveLocation(this.locations, locId);
    
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
    const parentId = this.getParentId(locId);
    let targetArray: StorageLocation[];

    if (parentId) {
      const parent = this.flatLocations.find(l => l.id === parentId);
      if (!parent || !parent.children) return;
      targetArray = parent.children;
    } else {
      targetArray = this.locations;
    }

    const index = targetArray.findIndex(l => l.id === locId);
    if (index === -1) return;

    const newIndex = direction === 'up' ? index - 1 : index + 1;

    if (newIndex < 0 || newIndex >= targetArray.length) return;

    const temp = targetArray[index];
    targetArray[index] = targetArray[newIndex];
    targetArray[newIndex] = temp;

    if (parentId) {
      const parent = this.flatLocations.find(l => l.id === parentId);
      if (parent) parent.children = [...targetArray];
    } else {
      this.locations = [...this.locations];
    }
    
    this.refreshState();

    const orderedIds = targetArray.map(l => l.id);
    this.reorderLocations(orderedIds, parentId).subscribe();
  }

  moveLocationApi(locId: string, targetId: string | null): Observable<any> {
    return this.locationService.move(locId, targetId);
  }

  private findAndRemoveLocation(tree: StorageLocation[], id: string): StorageLocation | null {
    for (let i = 0; i < tree.length; i++) {
      if (tree[i].id === id) return tree.splice(i, 1)[0];
      const children = tree[i].children;
      if (children && children.length > 0) {
        const found = this.findAndRemoveLocation(children, id);
        if (found) return found;
      }
    }
    return null;
  }

  moveItemApi(itemId: string, targetLocId: string) {
    return this.itemService.move(itemId, targetLocId);
  }

  reorderLocations(orderedIds: string[], parentId: string | null = null) {
    return this.locationService.reorder({ parentId, orderedIds });
  }

  moveLocation(locId: string, targetId: string | null): Observable<any> {
    const normalizedTargetId = (targetId === 'root' || !targetId) ? null : targetId;
    const currentParentId = this.getParentId(locId);

    if (currentParentId === normalizedTargetId) {
      return of(null);
    }

    if (!this.canMoveLocation(locId, normalizedTargetId)) {
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

  returnItem(itemId: string, dto: ReturnItemDto = { returnedDate: new Date().toISOString() }) {
    return this.lendingService.returnItem(itemId, dto);
  }

  isChildOf(targetId: string, sourceLoc: StorageLocation): boolean {
    return sourceLoc.children?.some(c => c.id === targetId || this.isChildOf(targetId, c)) || false;
  }

  private removeLocationFromTree(tree: StorageLocation[], id: string): boolean {
    for (let i = 0; i < tree.length; i++) {
      if (tree[i].id === id) { tree.splice(i, 1); return true; }
      if (tree[i].children?.length && this.removeLocationFromTree(tree[i].children!, id)) return true;
    }
    return false;
  }

  private getSubtreeDepth(loc: StorageLocation): number {
    if (!loc.children || loc.children.length === 0) return 1;
    return 1 + Math.max(...loc.children.map(c => this.getSubtreeDepth(c)));
  }

  public canMoveLocation(locId: string, targetId: string | null): boolean {
    const movingLoc = this.flatLocations.find(l => l.id === locId);
    if (!movingLoc) return false;

    const targetLevel = this.getLocationLevel(targetId);
    const movingDepth = this.getSubtreeDepth(movingLoc);

    return (targetLevel + movingDepth) <= 3;
  }
}