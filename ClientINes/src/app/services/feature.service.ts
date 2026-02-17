import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class FeatureService {
  isSalesModeEnabled = signal(localStorage.getItem('salesMode') === 'true');
  isLendingModeEnabled = signal(localStorage.getItem('lendingMode') === 'true');

  toggleSalesMode(value: boolean) {
    this.isSalesModeEnabled.set(value);
    localStorage.setItem('salesMode', String(value));
  }

  toggleLendingMode(enabled: boolean) {
    this.isLendingModeEnabled.set(enabled);
    localStorage.setItem('lendingMode', String(enabled));
  }
}