import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RouterModule, Router } from '@angular/router';
import { finalize, forkJoin, interval, Subscription } from 'rxjs';
import { takeWhile } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { FeatureService } from '../../../../core/services/feature.service';
import { CategoryService } from '../../../category/services/category.service';
import { PlatformService } from '../../../platform/services/platform.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { TelegramBotService } from '../../services/telegram-bot.service';
import { TelegramStatusContract } from '../../contracts/telegram-status';
import { TutorialService } from '../../../../core/services/tutorial.service';
import { AuthService } from '../../../auth/services/auth.service';

interface SimpleContract {
  id: string;
  name: string;
  color?: string;
}

type SettingsTab = 'general' | 'categories' | 'platforms' | 'integrations';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit, OnDestroy {
  public featureService = inject(FeatureService);
  private categoryService = inject(CategoryService);
  private platformService = inject(PlatformService);
  private modalService = inject(DashboardModalService);
  private translate = inject(TranslateService);
  private telegramService = inject(TelegramBotService);
  private tutorialService = inject(TutorialService);
  private authService = inject(AuthService);
  private toastr = inject(ToastrService);
  private router = inject(Router);

  categories: SimpleContract[] = [];
  platforms: SimpleContract[] = [];
  activeTab: SettingsTab = 'general';
  isLoading = false;

  tgStatus: TelegramStatusContract = { isLinked: false };
  isViberBotEnabled = false;

  private pollSubscription?: Subscription;

  ngOnInit() {
    this.loadAllData();
  }

  ngOnDestroy() {
    this.stopPolling();
  }

  loadAllData() {
    this.isLoading = true;
    forkJoin({
      categories: this.categoryService.getAll(),
      platforms: this.platformService.getAll()
    }).pipe(
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: (res) => {
        this.categories = res.categories;
        this.platforms = res.platforms;
      }
    });
  }

  loadTelegramStatus(showSpinner = true) {
    if (showSpinner) this.isLoading = true;

    this.telegramService.getStatus()
      .pipe(finalize(() => { if (showSpinner) this.isLoading = false; }))
      .subscribe(status => {
        this.tgStatus = status;
        if (status.isLinked) {
          this.stopPolling();
        }
      });
  }

  generateTelegramToken() {
    this.isLoading = true;
    this.telegramService.generateToken()
      .pipe(finalize(() => this.isLoading = false))
      .subscribe(status => {
        this.tgStatus = status;
        this.startPolling();
      });
  }

  onLaunchBotClick() {
    this.startPolling();
  }

  private startPolling() {
    this.stopPolling();
    this.pollSubscription = interval(2500)
      .pipe(takeWhile(() => !this.tgStatus.isLinked))
      .subscribe(() => {
        this.loadTelegramStatus(false);
      });
  }

  private stopPolling() {
    if (this.pollSubscription) {
      this.pollSubscription.unsubscribe();
      this.pollSubscription = undefined;
    }
  }

  unlinkTelegram() {
    this.modalService.openConfirm({
      mode: 'delete',
      title: 'COMMON.DELETE',
      message: this.translate.instant('SETTINGS_PAGE.INTEGRATIONS.UNLINK_CONFIRM'),
      name: 'Telegram Bot'
    }).subscribe(confirm => {
      if (confirm) {
        this.isLoading = true;
        this.telegramService.unlink()
          .pipe(finalize(() => this.isLoading = false))
          .subscribe(() => {
            this.tgStatus = { isLinked: false };
            this.stopPolling();
          });
      }
    });
  }

  getTelegramLink(): string {
    if (!this.tgStatus.botUsername || !this.tgStatus.verificationToken) return '#';
    return `https://t.me/${this.tgStatus.botUsername}?start=${this.tgStatus.verificationToken}`;
  }

  resetTutorials() {
    this.modalService.openConfirm({
      mode: 'confirm',
      title: 'COMMON.CONFIRM',
      message: this.translate.instant('SETTINGS_PAGE.RESET_TUTORIAL_CONFIRM'),
      confirmText: 'COMMON.YES'
    }).subscribe(confirm => {
      if (confirm) {
        this.isLoading = true;
        this.tutorialService.resetTutorialsOnBackend()
          .pipe(finalize(() => this.isLoading = false))
          .subscribe();
      }
    });
  }

  deleteAccount() {
    this.modalService.openConfirm({
      mode: 'delete',
      title: 'SETTINGS_PAGE.DELETE_ACCOUNT_TITLE',
      message: this.translate.instant('SETTINGS_PAGE.DELETE_ACCOUNT_CONFIRM')
    }).subscribe(confirm => {
      if (confirm) {
        this.isLoading = true;
        this.authService.deleteAccount()
          .pipe(finalize(() => this.isLoading = false))
          .subscribe({
            next: () => {
              this.toastr.success(this.translate.instant('AUTH.SUCCESS.ACCOUNT_DELETED'));
              this.router.navigate(['/auth/login']);
            },
            error: () => {
              this.toastr.error(this.translate.instant('SYSTEM.DEFAULT_ERROR'));
            }
          });
      }
    });
  }

  toggleViberBot(enabled: boolean): void {
    this.isViberBotEnabled = enabled;
  }

  addCategory() { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.CREATE', message: '', name: ''
    }).subscribe(res => {
      if (res) {
        this.categoryService.create({ name: res }).subscribe(() => this.loadAllData());
      }
    });
  }
  
  renameCategory(cat: SimpleContract) { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.EDIT', message: '', name: cat.name
    }).subscribe(res => {
      if (res) {
        this.categoryService.rename(cat.id, res).subscribe(() => this.loadAllData());
      }
    });
  }
  
  deleteCategory(cat: SimpleContract) { 
    this.modalService.openConfirm({
      mode: 'delete', 
      title: 'COMMON.DELETE', 
      message: this.translate.instant('SETTINGS_PAGE.MODAL.DELETE_CATEGORY'),
      name: cat.name
    }).subscribe(res => {
      if (res) {
        this.categoryService.delete(cat.id).subscribe(() => this.loadAllData());
      }
    });
  }

  addPlatform() { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.CREATE', message: '', name: ''
    }).subscribe(res => {
      if (res) {
        this.platformService.create({ name: res }).subscribe(() => this.loadAllData());
      }
    });
  }
  
  renamePlatform(plat: SimpleContract) { 
    this.modalService.openConfirm({
      mode: 'input', title: 'COMMON.EDIT', message: '', name: plat.name
    }).subscribe(res => {
      if (res) {
        this.platformService.rename(plat.id, res).subscribe(() => this.loadAllData());
      }
    });
  }
  
  deletePlatform(plat: SimpleContract) { 
    this.modalService.openConfirm({
      mode: 'delete',
      title: 'COMMON.DELETE',
      message: this.translate.instant('SETTINGS_PAGE.MODAL.DELETE_PLATFORM'),
      name: plat.name
    }).subscribe(res => {
      if (res) {
        this.platformService.delete(plat.id).subscribe(() => this.loadAllData());
      }
    });
  }

  switchTab(tab: SettingsTab) {
    this.activeTab = tab;
    if (tab === 'integrations') {
      this.loadTelegramStatus();
    } else {
      this.stopPolling();
    }
  }
}