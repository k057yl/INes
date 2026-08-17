import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StatsListModalComponent } from './stats-list-modal.component';
import { DashboardFacade } from '../dashboard/dashboard.facade';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { ItemStatus } from '../../../item/enums/item-status.enum';

describe('StatsListModalComponent', () => {
  let component: StatsListModalComponent;
  let fixture: ComponentFixture<StatsListModalComponent>;
  let facadeSpy: jasmine.SpyObj<DashboardFacade>;

  beforeEach(async () => {
    facadeSpy = jasmine.createSpyObj('DashboardFacade', [], {
      locations: {
        flatLocations: [
          {
            name: 'Гараж',
            items: [
              { id: 'item-1', status: ItemStatus.Lent },
              { id: 'item-2', status: ItemStatus.Active }
            ]
          }
        ]
      } as any,
      stats: {
        attentionItems: [
          { itemId: '1', severity: 'danger' },
          { itemId: '2', severity: 'warning' }
        ]
      } as any
    });

    await TestBed.configureTestingModule({
      imports: [StatsListModalComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: DashboardFacade, useValue: facadeSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(StatsListModalComponent);
    component = fixture.componentInstance;
  });

  it('lentItems должен отфильтровывать только сданные/арендованные вещи', () => {
    const lent = component.lentItems;
    expect(lent.length).toBe(1);
    expect(lent[0].item.id).toBe('item-1');
  });

  it('expiredAttentionItems должен возвращать только элементы с severity === danger', () => {
    const expired = component.expiredAttentionItems;
    expect(expired.length).toBe(1);
    expect(expired[0].itemId).toBe('1');
  });
});