import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { driver, Driver } from 'driver.js';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { DashboardModalService } from '../../features/dashboard/components/dashboard/dashboard.modal.service';
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
  private toastr = inject(ToastrService);
  private modalService = inject(DashboardModalService);
  private http = inject(HttpClient);
  
  private driverObj?: Driver;
  private isExplicitlyCompleted = false;
  private isCancellingToConfirm = false;

  private initDriver(step: TutorialStep, onCompleteCallback?: () => void) {
    this.isExplicitlyCompleted = false;

    this.driverObj = driver({
      showProgress: true,
      progressText: this.translate.instant('TUTORIAL_PAGE.PROGRESS'),
      animate: true,
      allowClose: false,
      doneBtnText: this.translate.instant('TUTORIAL_PAGE.DONE_BTN'),
      nextBtnText: this.translate.instant('TUTORIAL_PAGE.NEXT_BTN'),
      prevBtnText: this.translate.instant('TUTORIAL_PAGE.PREV_BTN'),
      popoverClass: 'inest-tutorial-popover',

      onPopoverRender: (popover: any) => {
        const footer = popover.wrapper.querySelector('.driver-popover-footer');
        
        if (footer && !footer.querySelector('.tutorial-skip-row')) {
          const skipRow = document.createElement('label');
          skipRow.className = 'tutorial-skip-row';

          const textSpan = document.createElement('span');
          textSpan.innerText = this.translate.instant('TUTORIAL_PAGE.SKIP_BTN');

          const checkbox = document.createElement('input');
          checkbox.type = 'checkbox';
          checkbox.className = 'skip-checkbox';

          skipRow.appendChild(textSpan);
          skipRow.appendChild(checkbox);

          checkbox.addEventListener('change', (e) => {
            e.stopPropagation();
            if (!checkbox.checked) return;

            checkbox.checked = false;

            this.modalService.openConfirm({
              mode: 'confirm',
              title: this.translate.instant('TUTORIAL_PAGE.SKIP_CONFIRM_TITLE'),
              message: this.translate.instant('TUTORIAL_PAGE.SKIP_CONFIRM_DESC'),
              confirmText: this.translate.instant('COMMON.YES')
            }).subscribe(confirmed => {
              if (!confirmed) return;

              this.isExplicitlyCompleted = true;
              this.markStepAsCompleted(step).subscribe();
              this.driverObj?.destroy();
              if (onCompleteCallback) onCompleteCallback();
            });
          });

          footer.appendChild(skipRow);
        }
      },

      onDestroyStarted: () => {
        if (!this.isExplicitlyCompleted) {
          this.isExplicitlyCompleted = true;
          this.markStepAsCompleted(step).subscribe();
          if (onCompleteCallback) onCompleteCallback();
        }
        this.driverObj?.destroy();
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

    // 1. Хедер
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

    // 2. Панель статистики
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

    // 3.1. Кнопка "Локации"
    if (document.querySelector('.stats-ribbon .stat-card:nth-child(1)')) {
      steps.push({
        element: '.stats-ribbon .stat-card:nth-child(1)',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_LOCATIONS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_LOCATIONS_DESC'),
          side: 'bottom'
        }
      });
    }

    // 3.2. Кнопка "Всего вещей"
    if (document.querySelector('.stats-ribbon .stat-card:nth-child(2)')) {
      steps.push({
        element: '.stats-ribbon .stat-card:nth-child(2)',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_ITEMS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_ITEMS_DESC'),
          side: 'bottom'
        }
      });
    }

    // 3.3. Кнопка "Продано"
    if (document.querySelector('.stats-ribbon .stat-card:nth-child(3)')) {
      steps.push({
        element: '.stats-ribbon .stat-card:nth-child(3)',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_SALES_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_SALES_DESC'),
          side: 'bottom'
        }
      });
    }

    // 3.4. Кнопка "Одолжено"
    if (document.querySelector('.stats-ribbon .stat-card:nth-child(4)')) {
      steps.push({
        element: '.stats-ribbon .stat-card:nth-child(4)',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_LENT_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_LENT_DESC'),
          side: 'bottom'
        }
      });
    }

    // 3.5. Ахтунг-карточка (если есть)
    if (document.querySelector('.attention-card')) {
      steps.push({
        element: '.attention-card',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_ATTENTION_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_ATTENTION_DESC'),
          side: 'bottom'
        }
      });
    }

    // 4. Лента навигации (Риббон)
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

    // 5. Карточки или Zero State
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

    // 6. Сам Футер
    if (document.querySelector('footer, .footer')) {
      steps.push({
        element: 'footer, .footer',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.FOOTER_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.FOOTER_DESC'),
          side: 'top'
        }
      });
    }

    // 7. ТОЧНЫЙ СЕЛЕКТОР: Кнопка фидбека в футере!
    if (document.querySelector('.feedback-btn')) {
      steps.push({
        element: '.feedback-btn',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.FEEDBACK_BTN_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.FEEDBACK_BTN_DESC'),
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

    if (document.querySelector('.custom-picker-tile')) {
      steps.push({
        element: '.custom-picker-tile',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.CUSTOM_COLOR_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.CUSTOM_COLOR_DESC'),
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
      }
    ];

    // 1. Фото
    if (document.querySelector('.photo-hero-section, .upload-placeholder')) {
      steps.push({
        element: '.photo-hero-section, .upload-placeholder',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.PHOTO_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.PHOTO_DESC'),
          side: 'bottom'
        }
      });
    }

    // 2. Имя
    if (document.querySelector('input[formControlName="name"]')) {
      steps.push({
        element: 'input[formControlName="name"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.NAME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.NAME_DESC'),
          side: 'bottom'
        }
      });
    }

    // 3. Описание
    if (document.querySelector('textarea[formControlName="description"]')) {
      steps.push({
        element: 'textarea[formControlName="description"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.DESC_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.DESC_DESC'),
          side: 'bottom'
        }
      });
    }

    // 4. Добавить категорию (Инлайн-кнопка)
    if (document.querySelector('.inline-add-btn')) {
      steps.push({
        element: '.inline-add-btn',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.ADD_CATEGORY_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.ADD_CATEGORY_DESC'),
          side: 'left'
        }
      });
    }

    // 5. Категория (Селект)
    if (document.querySelector('select[formControlName="categoryId"]')) {
      steps.push({
        element: 'select[formControlName="categoryId"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.CATEGORY_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.CATEGORY_DESC'),
          side: 'bottom'
        }
      });
    }

    // 6. Статус
    if (document.querySelector('select[formControlName="status"]')) {
      steps.push({
        element: 'select[formControlName="status"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.STATUS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.STATUS_DESC'),
          side: 'top'
        }
      });
    }

    // 7. Добавить финансы
    if (document.querySelector('input[formControlName="addDetails"]')) {
      steps.push({
        element: 'input[formControlName="addDetails"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.FINANCIALS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.FINANCIALS_DESC'),
          side: 'top'
        }
      });
    }

    // 8. Добавить чек
    if (document.querySelector('input[formControlName="addReceipt"]')) {
      steps.push({
        element: 'input[formControlName="addReceipt"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.RECEIPT_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.RECEIPT_DESC'),
          side: 'top'
        }
      });
    }

    // 9. Добавить напоминание
    if (document.querySelector('input[formControlName="addReminder"]')) {
      steps.push({
        element: 'input[formControlName="addReminder"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.REMINDER_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.REMINDER_DESC'),
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