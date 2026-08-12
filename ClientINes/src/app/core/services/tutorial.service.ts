import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { driver, Driver } from 'driver.js';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../features/auth/services/auth.service';

export enum TutorialStep {
  Dashboard = 1,
  Items = 2,
  Locations = 4,
  Settings = 8
}

export type TutorialTour =
  | 'dashboard'
  | 'items-list'
  | 'sales-list'
  | 'sell-form'
  | 'location-form'
  | 'item-form'
  | 'first-location-card'
  | 'first-item-card';

@Injectable({
  providedIn: 'root'
})
export class TutorialService {
  private translate = inject(TranslateService);
  private toastr = inject(ToastrService);
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  
  private driverObj?: Driver;
  private isExplicitlyCompleted = false;

  private setTutorialLock(locked: boolean) {
    document.body.classList.toggle('tutorial-active', locked);
  }

  private forceCleanup() {
    this.setTutorialLock(false);

    document.querySelectorAll('.driver-overlay, .driver-popover, .driver-active-element').forEach(el => {
      el.classList.remove('driver-active-element');
      if (el.classList.contains('driver-overlay') || el.classList.contains('driver-popover')) {
        el.remove();
      }
    });
  }

  private initDriver(step: TutorialStep, tourType: TutorialTour, onCompleteCallback?: () => void) {
    if (this.driverObj) {
      this.driverObj.destroy();
    }
    this.forceCleanup();

    this.isExplicitlyCompleted = false;
    this.setTutorialLock(true);

    this.driverObj = driver({
      showProgress: true,
      progressText: this.translate.instant('TUTORIAL_PAGE.PROGRESS'),
      animate: true,
      allowClose: false,
      allowKeyboardControl: false,
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
            e.preventDefault();

            const wrapper = popover.wrapper as HTMLElement;
            const footerEl = wrapper.querySelector('.driver-popover-footer') as HTMLElement;
            if (!footerEl) return;

            const existingConfirm = footerEl.querySelector('.tutorial-inline-confirm');

            const closeConfirmBox = () => {
              checkbox.checked = false;
              existingConfirm?.remove();
              footerEl.querySelector('.tutorial-inline-confirm')?.remove();
            };

            if (!checkbox.checked) {
              closeConfirmBox();
              return;
            }

            if (!existingConfirm) {
              const confirmBox = document.createElement('div');
              confirmBox.className = 'tutorial-inline-confirm';
              confirmBox.innerHTML = `
                <div class="confirm-title">${this.translate.instant('TUTORIAL_PAGE.SKIP_CONFIRM_DESC')}</div>
                <div class="confirm-buttons">
                  <button class="btn-confirm-yes">${this.translate.instant('COMMON.YES')}</button>
                  <button class="btn-confirm-no">${this.translate.instant('COMMON.CANCEL')}</button>
                </div>
              `;

              footerEl.appendChild(confirmBox);

              confirmBox.querySelector('.btn-confirm-yes')?.addEventListener('click', (ev) => {
                ev.stopPropagation();
                this.isExplicitlyCompleted = true;
                this.markStepAsCompleted(step).subscribe();
                this.driverObj?.destroy();
                this.forceCleanup();
                
                if (onCompleteCallback) onCompleteCallback();
              });

              confirmBox.querySelector('.btn-confirm-no')?.addEventListener('click', (ev) => {
                ev.stopPropagation();
                closeConfirmBox();
              });
            }
          });

          footer.appendChild(skipRow);
        }
      },

      onDestroyStarted: () => {
        if (this.driverObj?.isLastStep() && !this.isExplicitlyCompleted) {
          this.isExplicitlyCompleted = true;
          this.markStepAsCompleted(step).subscribe();
          if (onCompleteCallback) onCompleteCallback();
        }
        
        this.driverObj?.destroy();
        this.forceCleanup();
      }
    });
  }

  public markStepAsCompleted(step: TutorialStep) {
    this.authService.updateLocalUserTutorial(step);
    return this.http.post(`${environment.apiBaseUrl}/auth/complete-tutorial`, { step });
  }

  // --- 1. ТУР ПО ДАШБОРДУ ---
  public startDashboardTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Dashboard, 'dashboard', onComplete);
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
    if (document.querySelector('.header-main')) {
      steps.push({
        element: '.header-main',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.HEADER_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.HEADER_DESC'),
          side: 'bottom'
        }
      });
    }

    // 2. Вся панель статистики
    if (document.querySelector('.stats-ribbon')) {
      steps.push({
        element: '.stats-ribbon',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.STATS_DESC'),
          side: 'bottom'
        }
      });
    }

    // 3-6. Отдельные карточки статистики
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

    // 7. Карточка внимания
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

    // 8. Лента навигации (РИББОН ЛОКАЦИИ)
    if (document.querySelector('.ribbon-container')) {
      steps.push({
        element: '.ribbon-container',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.LOCATIONS_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.LOCATIONS_DESC'),
          side: 'bottom'
        }
      });
    }

    // 9. Футер и Обратная связь
    if (document.querySelector('.footer-content')) {
      steps.push({
        element: '.footer-content',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.FOOTER_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.FOOTER_DESC'),
          side: 'top'
        }
      });
    }

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

    // 10. КУЛЬМИНАЦИЯ: Интерактивный клик по кнопке создания
    const targetBtn = document.querySelector('.zero-state-container .tutorial-action, .tutorial-action');

    if (targetBtn) {
      steps.push({
        element: targetBtn,
        disableActiveInteraction: false,
        advanceOnClick: true,
        popover: {
          title: this.translate.instant('MAIN_PAGE.LOCATION_EMPTY_MSG'),
          description: this.translate.instant('TUTORIAL_PAGE.DASHBOARD.CREATE_FIRST_DESC'),
          side: 'top',
          showButtons: ['close']
        }
      });
    }

    this.driverObj.setSteps(steps);
    this.driverObj.drive();
  }

  // --- 2. ТУР ПО СПИСКУ ПРЕДМЕТОВ ---
  public startItemsListTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Items, 'items-list', onComplete);
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
    this.initDriver(TutorialStep.Items, 'sales-list', onComplete);
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
    this.initDriver(TutorialStep.Locations, 'location-form', onComplete);
    if (!this.driverObj) return;

    const steps: any[] = [
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.LOCATION_FORM.WELCOME_DESC'),
        }
      }
    ];

    if (document.querySelector('.create-card input[formControlName="name"]')) {
      steps.push({
        element: '.create-card input[formControlName="name"]',
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
    this.initDriver(TutorialStep.Items, 'item-form', onComplete);
    if (!this.driverObj) return;

    const steps: any[] = [
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.ITEM_FORM.WELCOME_DESC'),
        }
      }
    ];

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

  // --- 6. ТУР ПО ПЕРВОЙ КАРТОЧКЕ ЛОКАЦИИ ---
  public startFirstLocationCardTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Locations, 'first-location-card', onComplete);
    if (!this.driverObj) return;

    // Ждем микротаск для прорисовки DOM
    setTimeout(() => {
      const steps: any[] = [
        {
          popover: {
            title: this.translate.instant('TUTORIAL_PAGE.FIRST_LOCATION_CARD.WELCOME_TITLE'),
            description: this.translate.instant('TUTORIAL_PAGE.FIRST_LOCATION_CARD.WELCOME_DESC'),
          }
        }
      ];

      const cardHeader = document.querySelector('.location-column .loc-header-wrapper, .location-column');
      if (cardHeader) {
        steps.push({
          element: cardHeader,
          popover: {
            title: this.translate.instant('TUTORIAL_PAGE.FIRST_LOCATION_CARD.HEADER_TITLE'),
            description: this.translate.instant('TUTORIAL_PAGE.FIRST_LOCATION_CARD.HEADER_DESC'),
            side: 'bottom'
          }
        });
      }

      const addLocBtn = document.querySelector('.location-column .action-btn.add-loc');
      if (addLocBtn) {
        steps.push({
          element: addLocBtn,
          popover: {
            title: this.translate.instant('TUTORIAL_PAGE.FIRST_LOCATION_CARD.ADD_NESTED_TITLE'),
            description: this.translate.instant('TUTORIAL_PAGE.FIRST_LOCATION_CARD.ADD_NESTED_DESC'),
            side: 'top'
          }
        });
      }

      const addItemBtn = document.querySelector('.location-column .action-btn.add-item, .empty-drop-zone');
      if (addItemBtn) {
        steps.push({
          element: addItemBtn,
          popover: {
            title: this.translate.instant('TUTORIAL_PAGE.FIRST_LOCATION_CARD.ADD_ITEM_TITLE'),
            description: this.translate.instant('TUTORIAL_PAGE.FIRST_LOCATION_CARD.ADD_ITEM_DESC'),
            side: 'top'
          }
        });
      }

      this.driverObj?.setSteps(steps);
      this.driverObj?.drive();
    }, 100);
  }

  // --- 7. ТУР ПО ПЕРВОЙ КАРТОЧКЕ ПРЕДМЕТА ---
  public startFirstItemCardTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Items, 'first-item-card', onComplete);
    if (!this.driverObj) return;

    const steps: any[] = [
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.FIRST_ITEM_CARD.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.FIRST_ITEM_CARD.WELCOME_DESC'),
        }
      }
    ];

    // 1. Карточка предмета
    const itemCard = document.querySelector('app-item-card');
    if (itemCard) {
      steps.push({
        element: itemCard,
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.FIRST_ITEM_CARD.DRAG_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.FIRST_ITEM_CARD.DRAG_DESC'),
          side: 'bottom'
        }
      });
    }

    // 2. Инфо/Меню на предмет
    const menuBtn = document.querySelector('app-item-card .tool-btn, app-item-card button');
    if (menuBtn) {
      steps.push({
        element: menuBtn,
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.FIRST_ITEM_CARD.MENU_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.FIRST_ITEM_CARD.MENU_DESC'),
          side: 'left'
        }
      });
    }

    this.driverObj.setSteps(steps);
    this.driverObj.drive();
  }

  // --- 8. ТУР ПО МОДАЛКЕ ПРОДАЖИ ПРЕДМЕТА ---
  public startSellFormTour(onComplete?: () => void) {
    this.initDriver(TutorialStep.Items, 'sell-form', onComplete);
    if (!this.driverObj) return;

    const steps: any[] = [
      {
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.SELL_FORM.WELCOME_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.SELL_FORM.WELCOME_DESC'),
        }
      }
    ];

    if (document.querySelector('input[formControlName="salePrice"]')) {
      steps.push({
        element: 'input[formControlName="salePrice"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.SELL_FORM.PRICE_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.SELL_FORM.PRICE_DESC'),
          side: 'bottom'
        }
      });
    }

    if (document.querySelector('input[formControlName="soldDate"]')) {
      steps.push({
        element: 'input[formControlName="soldDate"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.SELL_FORM.DATE_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.SELL_FORM.DATE_DESC'),
          side: 'bottom'
        }
      });
    }

    if (document.querySelector('select[formControlName="platformId"]')) {
      steps.push({
        element: 'select[formControlName="platformId"]',
        popover: {
          title: this.translate.instant('TUTORIAL_PAGE.SELL_FORM.PLATFORM_TITLE'),
          description: this.translate.instant('TUTORIAL_PAGE.SELL_FORM.PLATFORM_DESC'),
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