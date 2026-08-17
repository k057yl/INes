import { DashboardNavigationService } from './dashboard-navigation.service';
import { StorageLocation } from '../../location/contracts/storage-location';
import { BOARD_CONFIG } from '../../../shared/constants/ui.constants';

describe('DashboardNavigationService', () => {
  let service: DashboardNavigationService;

  const mockLocations: StorageLocation[] = Array.from({ length: 25 }, (_, i) => ({
    id: `loc-${i}`,
    name: `Локация ${i}`
  } as StorageLocation));

  beforeEach(() => {
    service = new DashboardNavigationService();
  });

  it('jumpToLocation должен вычислять нужную страницу доски по ID локации', () => {
    const targetLocId = 'loc-12';
    const expectedPage = Math.floor(12 / BOARD_CONFIG.PAGE_SIZE);

    service.jumpToLocation(targetLocId, mockLocations);

    expect(service.currentPageBoard).toBe(expectedPage);
  });

  it('adjustPageAfterDelete должен уменьшать текущую страницу, если она стала пустой', () => {
    service.currentPageBoard = 2;

    service.adjustPageAfterDelete(0);

    expect(service.currentPageBoard).toBe(1);
  });
});