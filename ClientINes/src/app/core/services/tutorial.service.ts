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

  // --- 1. ТУР ПО ДАШБОРДУ ---
  public startDashboardTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Dashboard, onComplete);
    if (!this.driverObj) return;

    const steps: any[] = [
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.WELCOME_DESC'),
        }
      }
    ];

    if (document.querySelector('app-header, .app-header, header')) {
      steps.push({
        element: 'app-header, .app-header, header',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.HEADER_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.HEADER_DESC'),
          side: 'bottom'
        }
      });
    }

    if (document.querySelector('app-dashboard-stats')) {
      steps.push({
        element: 'app-dashboard-stats',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_DESC'),
          side: 'bottom'
        }
      });
    }

    // РИББОН (ЛЕНТА) ИДЕТ СНАЧАЛА!
    if (document.querySelector('app-location-ribbon')) {
      steps.push({
        element: 'app-location-ribbon',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.LOCATIONS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.LOCATIONS_DESC'),
          side: 'bottom'
        }
      });
    }

    // ДОСКА (КАРТОЧКИ) ИДЕТ ПОСЛЕ РИББОНА!
    if (document.querySelector('.root-loc-wrapper')) {
      steps.push({
        element: '.root-loc-wrapper:first-child',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.BOARD_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.BOARD_DESC'),
          side: 'right'
        }
      });
    } else if (document.querySelector('.zero-state-container')) {
      steps.push({
        element: '.zero-state-container',
        popover: {
          title: this.translate.instant('MAIN_PAGE.LOCATION_EMPTY_MSG'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.CREATE_FIRST_DESC'),
          side: 'top'
        }
      });
    }

    if (document.querySelector('app-footer, .app-footer, footer')) {
      steps.push({
        element: 'app-footer, .app-footer, footer',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.FOOTER_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.FOOTER_DESC'),
          side: 'top'
        }
      });
    }

    this.driverObj.setSteps(steps);
    this.driverObj.drive();
  }

  // --- 2. ТУР ПО СПИСКУ ПРЕДМЕТОВ ---
  public startItemsListTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Items, onComplete);
    if (!this.driverObj) return;

    const steps: any[] = [
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEMS_LIST.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEMS_LIST.WELCOME_DESC'),
        }
      }
    ];

    if (document.querySelector('.controls-toolbar')) {
      steps.push({
        element: '.controls-toolbar',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEMS_LIST.FILTERS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEMS_LIST.FILTERS_DESC'),
          side: 'bottom'
        }
      });
    }

    if (document.querySelector('.items-table-wrapper')) {
      steps.push({
        element: '.items-table-wrapper',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEMS_LIST.TABLE_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEMS_LIST.TABLE_DESC'),
          side: 'top'
        }
      });
    }

    this.driverObj.setSteps(steps);
    this.driverObj.drive();
  }

  // --- 3. ТУР ПО ПРОДАЖАМ ---
  public startSalesListTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Locations, onComplete);
    if (!this.driverObj) return;

    const steps: any[] = [
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.SALES_LIST.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.SALES_LIST.WELCOME_DESC'),
        }
      }
    ];

    if (document.querySelector('.stats-grid')) {
      steps.push({
        element: '.stats-grid',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.SALES_LIST.FINANCE_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.SALES_LIST.FINANCE_DESC'),
          side: 'bottom'
        }
      });
    }

    if (document.querySelector('.sales-grid')) {
      steps.push({
        element: '.sales-grid',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.SALES_LIST.ACTIONS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.SALES_LIST.ACTIONS_DESC'),
          side: 'top'
        }
      });
    }

    this.driverObj.setSteps(steps);
    this.driverObj.drive();
  }

  // --- 4. ТУР ПО МОДАЛКЕ СОЗДАНИЯ ЛОКАЦИИ ---
  public startLocationFormTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Locations, onComplete);
    if (!this.driverObj) return;

    const steps: any[] = [
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.WELCOME_DESC'),
        }
      }
    ];

    if (document.querySelector('.create-card .form-group input')) {
      steps.push({
        element: '.create-card .form-group:first-of-type',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.NAME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.NAME_DESC'),
          side: 'bottom'
        }
      });
    }

    if (document.querySelector('.color-selector-container')) {
      steps.push({
        element: '.color-selector-container',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.COLOR_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.COLOR_DESC'),
          side: 'top'
        }
      });
    }

    this.driverObj.setSteps(steps);
    this.driverObj.drive();
  }

  // --- 5. ТУР ПО МОДАЛКЕ СОЗДАНИЯ/РЕДАКТИРОВАНИЯ ПРЕДМЕТА ---
  public startItemFormTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Items, onComplete);
    if (!this.driverObj) return;

    const steps: any[] = [
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.WELCOME_DESC'),
        }
      },
      {
        element: '.item-card-wide form .inest-form-group:first-child',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.MAIN_FIELDS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.MAIN_FIELDS_DESC'),
          side: 'bottom'
        }
      }
    ];

    if (document.querySelector('.photo-hero-section')) {
      steps.push({
        element: '.photo-hero-section',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.PHOTO_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.PHOTO_DESC'),
          side: 'top'
        }
      });
    }

    if (document.querySelector('.reminder-toggle-section')) {
      steps.push({
        element: '.reminder-toggle-section:last-of-type',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.REMINDERS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.REMINDERS_DESC'),
          side: 'top'
        }
      });
    }

    this.driverObj.setSteps(steps);
    this.driverObj.drive();
  }

  public resetTutorialsOnBackend() {
    return this.http.post(`${environment.apiBaseUrl}/auth/reset-tutorials`, {});
  }
}