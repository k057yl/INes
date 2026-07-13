import { Injectable } from '@angular/core';
import { StorageLocation } from '../../core/models/entities/storage-location.entity';

@Injectable()
export class DashboardTreeService {
  public flattenLocations(locs: StorageLocation[]): StorageLocation[] {
    return locs.reduce<StorageLocation[]>((acc, l) => {
      acc.push(l);
      if (l.children?.length) acc.push(...this.flattenLocations(l.children));
      return acc;
    }, []);
  }

  public excludeLocation(tree: StorageLocation[], id: string): StorageLocation[] {
    return tree
      .filter(l => l.id !== id)
      .map(l => ({ ...l, children: l.children ? this.excludeLocation(l.children, id) : [] }));
  }

  public getParentId(flatLocations: StorageLocation[], locId: string): string | null {
    return flatLocations.find(l => l.children?.some(c => c.id === locId))?.id || null;
  }

  public getLocationLevel(flatLocations: StorageLocation[], locId: string | null): number {
    if (!locId || locId === 'root') return 0;
    const pid = this.getParentId(flatLocations, locId);
    return 1 + (pid ? this.getLocationLevel(flatLocations, pid) : 0);
  }

  public getSubtreeDepth(loc: StorageLocation): number {
    if (!loc.children?.length) return 1;
    return 1 + Math.max(...loc.children.map(c => this.getSubtreeDepth(c)));
  }

  public canMoveLocation(flatLocations: StorageLocation[], locId: string, targetId: string | null): boolean {
    const movingLoc = flatLocations.find(l => l.id === locId);
    if (!movingLoc) return false;
    return (this.getLocationLevel(flatLocations, targetId) + this.getSubtreeDepth(movingLoc)) <= 3;
  }

  public isChildOf(targetId: string, sourceLoc: StorageLocation): boolean {
    return sourceLoc.children?.some(c => c.id === targetId || this.isChildOf(targetId, c)) || false;
  }
}