import { Injectable, inject } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { tap } from 'rxjs/operators';
import { StorageLocation } from '../../../core/contracts/storage-location';
import { LocationService } from '../../../core/services/location.service';
import { DashboardTreeService } from './dashboard-tree.service';

@Injectable()
export class DashboardLocationService {
  private locationApi = inject(LocationService);
  private treeService = inject(DashboardTreeService);

  locations: StorageLocation[] = [];
  flatLocations: StorageLocation[] = [];
  draggedLocationId: string | null = null;

  loadTree(): Observable<StorageLocation[]> {
    return this.locationApi.getTree().pipe(
      tap(data => {
        this.locations = data;
        this.refreshFlat();
      })
    );
  }

  refreshFlat() {
    this.flatLocations = this.treeService.flattenLocations(this.locations);
  }

  delete(id: string): Observable<void> {
    return this.locationApi.delete(id).pipe(
      tap(() => {
        this.locations = this.treeService.excludeLocation(this.locations, id);
        this.refreshFlat();
      })
    );
  }

  rename(id: string, newName: string): Observable<any> {
    return this.locationApi.rename(id, newName).pipe(
      tap(() => {
        const loc = this.flatLocations.find(l => l.id === id);
        if (loc) loc.name = newName;
      })
    );
  }

  moveLocally(locId: string, targetId: string | null) {
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
      this.refreshFlat();
    }
  }

  move(locId: string, targetId: string | null): Observable<any> {
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
      this.moveLocally(locId, normalizedTargetId);
    } catch {
      this.locations = previousLocations;
      return throwError(() => 'LOCAL_MOVE_FAILED');
    }

    return this.locationApi.move(locId, normalizedTargetId).pipe(
      tap({
        error: () => {
          this.locations = previousLocations;
          this.refreshFlat();
        }
      })
    );
  }

  moveUpDown(locId: string, direction: 'up' | 'down'): Observable<any> {
    const parentId = this.treeService.getParentId(this.flatLocations, locId);
    let targetArray = parentId
      ? this.flatLocations.find(l => l.id === parentId)?.children || []
      : this.locations;

    const index = targetArray.findIndex(l => l.id === locId);
    if (index === -1) return of(null);

    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex < 0 || newIndex >= targetArray.length) return of(null);

    [targetArray[index], targetArray[newIndex]] = [targetArray[newIndex], targetArray[index]];
    this.refreshFlat();

    const orderedIds = targetArray.map(l => l.id);
    return this.locationApi.reorder({ parentId, orderedIds });
  }

  reorder(orderedIds: string[], parentId: string | null = null): Observable<any> {
    return this.locationApi.reorder({ parentId, orderedIds });
  }
}