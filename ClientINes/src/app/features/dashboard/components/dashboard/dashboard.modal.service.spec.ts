import { TestBed } from '@angular/core/testing';
import { DashboardModalService } from './dashboard.modal.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';
import { Item } from '../../../item/contracts/item';

describe('DashboardModalService', () => {
  let service: DashboardModalService;
  let toastrSpy: jasmine.SpyObj<ToastrService>;

  beforeEach(() => {
    toastrSpy = jasmine.createSpyObj('ToastrService', ['warning']);
    const translateSpy = jasmine.createSpyObj('TranslateService', ['instant']);

    TestBed.configureTestingModule({
      providers: [
        DashboardModalService,
        { provide: ToastrService, useValue: toastrSpy },
        { provide: TranslateService, useValue: translateSpy }
      ]
    });

    service = TestBed.inject(DashboardModalService);
  });

  it('openItemForm не должен открывать форму для проданных/арендованных вещей (status !== 0)', () => {
    const soldItem = { id: 'item-1', status: 1 } as Item; // status 1 = Sold/Lent

    service.openItemForm(soldItem);

    expect(toastrSpy.warning).toHaveBeenCalled();
    expect(service.activeModal).toBeNull();
  });

  it('openConfirm должен устанавливать правильный конфиг и активную модалку', () => {
    service.openConfirm({ mode: 'delete', title: 'Удалить', message: 'Вы уверены?' });

    expect(service.activeModal).toBe('confirm');
    expect(service.config.confirmText).toBe('COMMON.CONFIRM');
  });

  it('close() должен сбрасывать состояние модалки', () => {
    service.activeModal = 'itemForm';
    service.close();

    expect(service.activeModal).toBeNull();
    expect(service.selectedItem).toBeNull();
  });
});