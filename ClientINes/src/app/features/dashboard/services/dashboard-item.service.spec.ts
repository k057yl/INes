import { TestBed } from '@angular/core/testing';
import { DashboardItemService } from './dashboard-item.service';
import { ItemService } from '../../item/services/item.service';
import { SalesService } from '../../sales/services/sales.service';
import { LendingService } from '../../lending/services/lending.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { StorageLocation } from '../../location/contracts/storage-location';
import { Item } from '../../item/contracts/item';
import { SaleListItem } from '../../sales/contracts/sale-list-item';

describe('DashboardItemService', () => {
  let service: DashboardItemService;
  let salesApiSpy: jasmine.SpyObj<SalesService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    const itemApiSpy = jasmine.createSpyObj('ItemService', ['archive', 'move', 'changeStatus']);
    salesApiSpy = jasmine.createSpyObj('SalesService', ['sellItem']);
    const lendingApiSpy = jasmine.createSpyObj('LendingService', ['lendItem', 'returnItem']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        DashboardItemService,
        { provide: ItemService, useValue: itemApiSpy },
        { provide: SalesService, useValue: salesApiSpy },
        { provide: LendingService, useValue: lendingApiSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });

    service = TestBed.inject(DashboardItemService);
  });

  it('moveLocally должен переносить вещь из массива одной локации в другую', () => {
    const testItem = { id: 'item-1', storageLocationId: 'loc-1' } as Item;
    const flatLocations: StorageLocation[] = [
      { id: 'loc-1', items: [testItem] } as unknown as StorageLocation,
      { id: 'loc-2', items: [] } as unknown as StorageLocation
    ];

    service.moveLocally(testItem, 'loc-2', flatLocations);

    expect(flatLocations[0].items?.length).toBe(0);
    expect(flatLocations[1].items?.length).toBe(1);
    expect(testItem.storageLocationId).toBe('loc-2');
  });

  it('sell() должен перенаправлять на /sales после успешной продажи', () => {
    const mockSaleResult: SaleListItem = {
      saleId: 'sale-1',
      itemId: 'item-1',
      itemName: 'Предмет',
      salePrice: 100,
      profit: 20,
      currency: 'USD',
      soldDate: '2026-08-17',
      platformName: 'OLX'
    };

    salesApiSpy.sellItem.and.returnValue(of(mockSaleResult));

    service.sell({ itemId: 'item-1', price: 100 } as any).subscribe(() => {
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/sales']);
    });
  });
});