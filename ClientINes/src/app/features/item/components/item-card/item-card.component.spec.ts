import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ItemCardComponent } from './item-card.component';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { Item } from '../../contracts/item';

describe('ItemCardComponent', () => {
  let component: ItemCardComponent;
  let fixture: ComponentFixture<ItemCardComponent>;

  const mockActiveItem = {
    id: 'item-1',
    name: 'Наушники',
    status: 0,
    storageLocationId: 'loc-1'
  } as Item;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ItemCardComponent, TranslateModule.forRoot()],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(ItemCardComponent);
    component = fixture.componentInstance;
    component.item = { ...mockActiveItem };
    fixture.detectChanges();
  });

  it('canSell и canLend должны быть true только для активного статуса', () => {
    expect(component.canSell).toBeTrue();
    expect(component.canLend).toBeTrue();

    component.item.status = 1; // Lent
    expect(component.canSell).toBeFalse();
    expect(component.canLend).toBeFalse();
  });

  it('confirmStatusChange должен эмитить событие смены статуса', () => {
    spyOn(component.statusChange, 'emit');
    component.pendingStatus = 2;

    component.confirmStatusChange();

    expect(component.statusChange.emit).toHaveBeenCalledWith({
      item: component.item,
      newStatus: 2
    });
    expect(component.showStatusModal).toBeFalse();
  });
});