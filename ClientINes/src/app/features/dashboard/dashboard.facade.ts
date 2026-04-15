import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { StorageLocation } from '../../models/entities/storage-location.entity';
import { Item } from '../../models/entities/item.entity';
import { LocationService } from '../../shared/services/location.service';
import { ItemService } from '../../shared/services/item.service';
import { SalesService } from '../../shared/services/sales.service';
import { LendingService } from '../../shared/services/lending.service';
import { SellItemRequestDto } from '../../models/dtos/sale.dto';
import { LendItemDto } from '../../models/dtos/lending.dto';

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
  isLoading = true;

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
  }

  private flattenLocations(locs: StorageLocation[]): StorageLocation[] {
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

  reorderLocations(orderedIds: string[]) {
    return this.locationService.reorder({ parentId: null, orderedIds });
  }

  moveLocation(locId: string, targetId: string) {
    const newParentId = targetId === 'root' ? null : targetId;
    return this.locationService.move(locId, newParentId);
  }

  sellItem(dto: SellItemRequestDto) {
    return this.salesService.sellItem(dto).pipe(
      tap(() => this.router.navigate(['/sales']))
    );
  }

  lendItem(dto: LendItemDto) {
    return this.lendingService.lendItem(dto);
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
}