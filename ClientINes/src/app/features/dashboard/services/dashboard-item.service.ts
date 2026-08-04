import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Item } from '../../item/contracts/item';
import { ItemService } from '../../item/services/item.service';
import { SalesService } from '../../sales/services/sales.service';
import { LendingService } from '../../lending/services/lending.service';
import { SaleCreateDto } from '../../sales/dtos/sale-item-create.dto';
import { ItemLendDto } from '../../item/dtos/item-lend.dto';
import { ItemReturnDto } from '../../item/dtos/item-return.dto';
import { StorageLocation } from '../../location/contracts/storage-location';

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