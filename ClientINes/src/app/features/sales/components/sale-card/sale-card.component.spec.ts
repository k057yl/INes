import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SaleCardComponent } from './sale-card.component';
import { TranslateModule } from '@ngx-translate/core';
import { CurrencyPipe } from '@angular/common';
import { SaleListItem } from '../../contracts/sale-list-item';

describe('SaleCardComponent', () => {
  let component: SaleCardComponent;
  let fixture: ComponentFixture<SaleCardComponent>;

  const mockSale: SaleListItem = {
    saleId: 's-1',
    itemId: 'item-100',
    itemName: 'Клавиатура',
    salePrice: 50,
    profit: 10,
    currency: 'USD',
    soldDate: '2026-08-17',
    platformName: 'OLX'
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SaleCardComponent, TranslateModule.forRoot()],
      providers: [CurrencyPipe]
    }).compileComponents();

    fixture = TestBed.createComponent(SaleCardComponent);
    component = fixture.componentInstance;
    component.sale = { ...mockSale };
    fixture.detectChanges();
  });

  it('isItemExists должен возвращать false, если itemId пустой или равен EMPTY_GUID', () => {
    expect(component.isItemExists).toBeTrue();

    component.sale.itemId = '00000000-0000-0000-0000-000000000000';
    expect(component.isItemExists).toBeFalse();
  });

  it('onUndo должен эмитить событие с объектом продажи', () => {
    spyOn(component.undo, 'emit');

    component.onUndo();

    expect(component.undo.emit).toHaveBeenCalledWith(component.sale);
  });
});