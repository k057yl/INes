import { Injectable, inject } from '@angular/core';
import { Subject, Observable, EMPTY } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';
import { StorageLocation } from '../../../location/contracts/storage-location';
import { Item } from '../../../item/contracts/item';

export type DashboardModalType = 'itemForm' | 'locationForm' | 'categoryForm' | 'platformForm' | 'confirm' | 'sell' | 'lend' | 'statsList' | 'feedback' | 'notEmptyLocationDelete' | null;

export type StatsListType = 'locations' | 'lent' | 'attention';

export type ConfirmModalMode = 'delete' | 'confirm' | 'input' | 'password';

@Injectable({ providedIn: 'root' })
export class DashboardModalService {
  activeModal: DashboardModalType = null;
  config: any = null;

  currentParentId: string | null = null;
  selectedItem: Item | null = null;
  selectedLocation: StorageLocation | null = null;
  selectedEntity: any = null; 

  statsType: StatsListType | null = null;
  
  private confirmSubj = new Subject<any>();
  private refreshDataSubj = new Subject<void>();
  private toastr = inject(ToastrService);
  private translateService = inject(TranslateService);
  public refreshData$ = this.refreshDataSubj.asObservable();

  openFeedback(): Observable<any> {
    this.activeModal = 'feedback';
    return this.resetSubject();
  }

  openStatsList(type: StatsListType): Observable<any> {
    console.log('СЕРВИС ПОЛУЧИЛ КОМАНДУ:', type);
    this.statsType = type;
    this.activeModal = 'statsList';
    return this.resetSubject();
  }

  openItemForm(item: Item | null = null, locationId: string | null = null): Observable<any> {
    if (item && item.status !== 0) {
      this.toastr.warning(this.translateService.instant('CREATE_ITEM_PAGE.WARNING_EDIT_MSG'));
      return EMPTY;
    }

    this.selectedItem = item;
    this.currentParentId = locationId;
    this.activeModal = 'itemForm';
    return this.resetSubject();
  }

  openLocationForm(loc: StorageLocation | null = null, parentId: string | null = null): Observable<any> {
    this.selectedLocation = loc;
    this.currentParentId = parentId;
    this.activeModal = 'locationForm';
    return this.resetSubject();
  }

  openCategoryForm(category: any = null): Observable<any> {
    this.selectedEntity = category;
    this.activeModal = 'categoryForm';
    return this.resetSubject();
  }

  openPlatformForm(platform: any = null): Observable<any> {
    this.selectedEntity = platform;
    this.activeModal = 'platformForm';
    return this.resetSubject();
  }

  openConfirm(config: { mode: ConfirmModalMode, title: string, message: string, name?: string, confirmText?: string, cancelText?: string }): Observable<any> {
    this.config = {
      ...config,
      confirmText: config.confirmText || 'COMMON.CONFIRM',
      cancelText: config.cancelText || 'COMMON.CANCEL'
    };
    this.activeModal = 'confirm';
    return this.resetSubject();
  }

  openSell(item: Item): Observable<any> {
    this.selectedItem = item;
    this.activeModal = 'sell';
    return this.resetSubject();
  }

  openLend(item: Item): Observable<any> {
    this.selectedItem = item;
    this.activeModal = 'lend';
    return this.resetSubject();
  }

  confirm(payload?: any) {
    this.confirmSubj.next(payload);
    this.refreshDataSubj.next();
    this.close();
  }

  close() {
    this.activeModal = null;
    this.config = null;
    this.selectedItem = null;
    this.selectedLocation = null;
    this.currentParentId = null;
    this.selectedEntity = null;
    this.statsType = null;
    if (!this.confirmSubj.closed) this.confirmSubj.complete();
  }

  private resetSubject(): Observable<any> {
    if (!this.confirmSubj.closed) this.confirmSubj.complete();
    this.confirmSubj = new Subject<any>();
    return this.confirmSubj.asObservable();
  }

  openNotEmptyLocationDelete(config: { locationId: string, availableLocations: StorageLocation[] }): Observable<string | null | undefined> {
    this.config = config;
    this.activeModal = 'notEmptyLocationDelete';
    return this.resetSubject();
  }
}