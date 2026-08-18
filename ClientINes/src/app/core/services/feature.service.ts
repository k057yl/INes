import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class FeatureService {
  private readonly SALES_KEY = 'salesMode';
  private readonly LENDING_KEY = 'lendingMode';

  isSalesModeEnabled = signal(
    localStorage.getItem(this.SALES_KEY) !== null 
      ? localStorage.getItem(this.SALES_KEY) === 'true' 
      : true
  );

  isLendingModeEnabled = signal(
    localStorage.getItem(this.LENDING_KEY) !== null 
      ? localStorage.getItem(this.LENDING_KEY) === 'true' 
      : true
  );

  toggleSalesMode(value: boolean) {
    this.isSalesModeEnabled.set(value);
    localStorage.setItem(this.SALES_KEY, String(value));
  }

  toggleLendingMode(enabled: boolean) {
    this.isLendingModeEnabled.set(enabled);
    localStorage.setItem(this.LENDING_KEY, String(enabled));
  }
}