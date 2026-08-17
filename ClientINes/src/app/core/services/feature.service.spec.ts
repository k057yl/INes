import { TestBed } from '@angular/core/testing';
import { FeatureService } from './feature.service';

describe('FeatureService', () => {
  let service: FeatureService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [FeatureService]
    });
    service = TestBed.inject(FeatureService);
  });

  it('toggleSalesMode() должен обновлять сигнал и localStorage', () => {
    service.toggleSalesMode(true);

    expect(service.isSalesModeEnabled()).toBeTrue();
    expect(localStorage.getItem('salesMode')).toBe('true');
  });

  it('toggleLendingMode() должен обновлять сигнал и localStorage', () => {
    service.toggleLendingMode(false);

    expect(service.isLendingModeEnabled()).toBeFalse();
    expect(localStorage.getItem('lendingMode')).toBe('false');
  });
});