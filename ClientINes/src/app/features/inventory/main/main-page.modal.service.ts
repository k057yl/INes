import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { StorageLocation } from '../../../models/entities/storage-location.entity';
import { Item } from '../../../models/entities/item.entity';

export type ModalType = | 'deleteItem' | 'deleteLoc' | 'renameLoc' | 'moveConfirm' | 'sell' | 'lend' | 'createItem' | 'createLoc' | null;

@Injectable({
  providedIn: 'root'
})
export class MainPageModalService {
  activeModal: ModalType = null;
  config: any = null;

  currentParentId: string | null = null;
  selectedItem: Item | null = null;
  selectedLocation: StorageLocation | null = null;
  
  private confirmSubj = new Subject<any>();

  openCreateItem(locationId: string | null = null): Observable<any> {
    this.currentParentId = locationId;
    this.activeModal = 'createItem';
    return this.resetSubject();
  }

  openCreateLocation(parentId: string | null = null): Observable<any> {
    this.currentParentId = parentId;
    this.activeModal = 'createLoc';
    return this.resetSubject();
  }

  openRename(loc: StorageLocation): Observable<string> {
    this.selectedLocation = loc;
    this.activeModal = 'renameLoc';
    this.config = { mode: 'input', title: 'COMMON.RENAME', message: '', confirmText: 'COMMON.SAVE', cancelText: 'COMMON.CANCEL', name: loc.name };
    return this.resetSubject();
  }

  openDeleteLocation(loc: StorageLocation): Observable<void> {
    this.selectedLocation = loc;
    this.activeModal = 'deleteLoc';
    this.config = { mode: 'delete', title: 'COMMON.DELETE', message: 'LOCATION_CARD.MODAL.YOU_SURE_MSG', confirmText: 'COMMON.DELETE', cancelText: 'COMMON.CANCEL' };
    return this.resetSubject();
  }

  openDeleteItem(item: Item): Observable<void> {
    this.selectedItem = item;
    this.activeModal = 'deleteItem';
    this.config = { mode: 'delete', title: 'COMMON.DELETE', message: 'ITEM_CARD.MODAL.YOU_SURE_MSG', confirmText: 'COMMON.DELETE', cancelText: 'COMMON.CANCEL' };
    return this.resetSubject();
  }

  openMoveConfirm(targetName: string): Observable<void> {
    this.activeModal = 'moveConfirm';
    this.config = { mode: 'confirm', title: 'ITEM_CARD.MODAL.MOVE_TITLE', message: targetName, name: 'skip', confirmText: 'COMMON.YES', cancelText: 'COMMON.NO' };
    return this.resetSubject();
  }

  openSell(item: Item): Observable<any> {
    this.selectedItem = item;
    this.activeModal = 'sell';
    this.config = null;
    return this.resetSubject();
  }

  openLend(item: Item): Observable<any> {
    this.selectedItem = item;
    this.activeModal = 'lend';
    this.config = null;
    return this.resetSubject();
  }

  confirm(payload?: any) {
    this.confirmSubj.next(payload);
    this.close();
  }

  close() {
    this.activeModal = null;
    this.config = null;
    this.selectedItem = null;
    this.selectedLocation = null;
    this.currentParentId = null;
    this.confirmSubj.complete();
  }

  private resetSubject(): Observable<any> {
    if (!this.confirmSubj.closed) {
      this.confirmSubj.complete();
    }
    this.confirmSubj = new Subject<any>();
    return this.confirmSubj.asObservable();
  }
}