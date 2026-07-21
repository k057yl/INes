import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Item } from '../../../core/contracts/item';
import { ItemService } from '../../../core/services/item.service';
import { SalesService } from '../../../core/services/sales.service';
import { LendingService } from '../../../core/services/lending.service';
import { SaleCreateDto } from '../../../core/dtos/sale-item-create.dto';
import { ItemLendDto } from '../../../core/dtos/item-lend.dto';
import { ItemReturnDto } from '../../../core/dtos/item-return.dto';
import { StorageLocation } from '../../../core/contracts/storage-location';

@Injectable()
export class DashboardItemService {
  private itemApi = inject(ItemService);
  private salesApi = inject(SalesService);
  private lendingApi = inject(LendingService);
  private router = inject(Router);

  delete(id: string): Observable<any> {
    return this.itemApi.archive(id);
  }

  moveLocally(item: Item, targetLocId: string, flatLocations: StorageLocation[]) {
    const sourceLoc = flatLocations.find(l => l.id === item.storageLocationId);
    const targetLoc = flatLocations.find(l => l.id === targetLocId);

    if (sourceLoc && targetLoc) {
      sourceLoc.items = (sourceLoc.items || []).filter(i => i.id !== item.id);
      item.storageLocationId = targetLocId;
      targetLoc.items = [...(targetLoc.items || []), item];
    }
  }

  moveApi(itemId: string, targetLocId: string): Observable<any> {
    return this.itemApi.move(itemId, targetLocId);
  }

  sell(dto: SaleCreateDto): Observable<any> {
    return this.salesApi.sellItem(dto).pipe(
      tap(() => this.router.navigate(['/sales']))
    );
  }

  lend(dto: ItemLendDto): Observable<any> {
    return this.lendingApi.lendItem(dto);
  }

  returnItem(itemId: string, dto: ItemReturnDto = { returnedDate: new Date().toISOString() }): Observable<any> {
    return this.lendingApi.returnItem(itemId, dto);
  }

  changeStatus(itemId: string, newStatus: number): Observable<any> {
    return this.itemApi.changeStatus(itemId, newStatus);
  }
}