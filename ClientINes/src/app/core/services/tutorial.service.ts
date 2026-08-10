import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { driver, Driver } from 'driver.js';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export enum TutorialStep {
  Dashboard = 1,
  Items = 2,
  Locations = 4,
  Settings = 8
}

@Injectable({
  providedIn: 'root'
})
export class TutorialService {
  private translate = inject(TranslateService);
  private http = inject(HttpClient);
  
  private driverObj?: Driver;

  private initDriver(step: TutorialStep, onCompleteCallback?: () => void) {
    this.driverObj = driver({
      showProgress: true,
      animate: true,
      allowClose: true,
      doneBtnText: this.translate.instant('TUTORIAL_PAGE.DONE_BTN'),
      nextBtnText: this.translate.instant('TUTORIAL_PAGE.NEXT_BTN'),
      prevBtnText: this.translate.instant('TUTORIAL_PAGE.PREV_BTN'),
      popoverClass: 'inest-tutorial-popover',
      
      onDestroyed: () => {
        this.markStepAsCompleted(step).subscribe();
        if (onCompleteCallback) onCompleteCallback();
      }
    });
  }

  public markStepAsCompleted(step: TutorialStep) {
    return this.http.post(`${environment.apiBaseUrl}/auth/complete-tutorial`, { step });
  }

  public startDashboardTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Dashboard, onComplete);

    if (!this.driverObj) return;

    this.driverObj.setSteps([
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.WELCOME_DESC'),
        }
      },
      {
        element: '.stats-summary-bar',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_DESC'),
          side: 'bottom'
        }
      },
      {
        element: '.location-column:first-child',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.LOCATIONS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.LOCATIONS_DESC'),
          side: 'right'
        }
      }
    ]);

    this.driverObj.drive();
  }

  public resetTutorialsOnBackend() {
    return this.http.post(`${environment.apiBaseUrl}/auth/reset-tutorials`, {});
  }
}