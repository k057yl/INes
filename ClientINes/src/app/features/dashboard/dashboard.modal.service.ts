import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { StorageLocation } from '../../models/entities/storage-location.entity';
import { Item } from '../../models/entities/item.entity';

export type DashboardModalType = 'itemForm' | 'locationForm' | 'categoryForm' | 'platformForm' | 'confirm' | 'sell' | 'lend' | null;

@Injectable({ providedIn: 'root' })
export class DashboardModalService {
  activeModal: DashboardModalType = null;
  config: any = null;

  currentParentId: string | null = null;
  selectedItem: Item | null = null;
  selectedLocation: StorageLocation | null = null;
  selectedEntity: any = null; 
  
  private confirmSubj = new Subject<any>();
  private refreshDataSubj = new Subject<void>();
  public refreshData$ = this.refreshDataSubj.asObservable();

  openItemForm(item: Item | null = null, locationId: string | null = null): Observable<any> {
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

  openConfirm(config: { mode: 'delete' | 'confirm' | 'input', title: string, message: string, name?: string, confirmText?: string, cancelText?: string }): Observable<any> {
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
    this.selectedEntity = null; // чистим
    if (!this.confirmSubj.closed) this.confirmSubj.complete();
  }

  private resetSubject(): Observable<any> {
    if (!this.confirmSubj.closed) this.confirmSubj.complete();
    this.confirmSubj = new Subject<any>();
    return this.confirmSubj.asObservable();
  }
}