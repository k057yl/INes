import { ComponentFixture, TestBed, fakeAsync, tick, discardPeriodicTasks } from '@angular/core/testing';
import { SettingsComponent } from './settings.component';
import { CategoryService } from '../../../category/services/category.service';
import { PlatformService } from '../../../platform/services/platform.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';
import { TelegramBotService } from '../../services/telegram-bot.service';
import { TutorialService } from '../../../../core/services/tutorial.service';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

describe('SettingsComponent', () => {
  let component: SettingsComponent;
  let fixture: ComponentFixture<SettingsComponent>;
  let telegramServiceSpy: jasmine.SpyObj<TelegramBotService>;
  let modalServiceSpy: jasmine.SpyObj<DashboardModalService>;

  beforeEach(async () => {
    telegramServiceSpy = jasmine.createSpyObj('TelegramBotService', ['getStatus', 'generateToken', 'unlink']);
    modalServiceSpy = jasmine.createSpyObj('DashboardModalService', ['openConfirm']);

    const categorySpy = jasmine.createSpyObj('CategoryService', ['getAll', 'create', 'rename', 'delete']);
    const platformSpy = jasmine.createSpyObj('PlatformService', ['getAll', 'create', 'rename', 'delete']);
    const tutorialSpy = jasmine.createSpyObj('TutorialService', ['resetTutorialsOnBackend']);

    categorySpy.getAll.and.returnValue(of([]));
    platformSpy.getAll.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [SettingsComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: TelegramBotService, useValue: telegramServiceSpy },
        { provide: DashboardModalService, useValue: modalServiceSpy },
        { provide: CategoryService, useValue: categorySpy },
        { provide: PlatformService, useValue: platformSpy },
        { provide: TutorialService, useValue: tutorialSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('generateTelegramToken должен запрашивать токен и запускать интервал опроса статуса', fakeAsync(() => {
    telegramServiceSpy.generateToken.and.returnValue(of({ isLinked: false, verificationToken: 'abc' }));
    telegramServiceSpy.getStatus.and.returnValue(of({ isLinked: false }));

    component.generateTelegramToken();

    expect(telegramServiceSpy.generateToken).toHaveBeenCalled();

    tick(2500);
    expect(telegramServiceSpy.getStatus).toHaveBeenCalled();

    discardPeriodicTasks();
  }));

  it('unlinkTelegram должен вызывать подтверждение и отвязывать бота', () => {
    modalServiceSpy.openConfirm.and.returnValue(of(true));
    telegramServiceSpy.unlink.and.returnValue(of(void 0));

    component.unlinkTelegram();

    expect(modalServiceSpy.openConfirm).toHaveBeenCalled();
    expect(telegramServiceSpy.unlink).toHaveBeenCalled();
    expect(component.tgStatus.isLinked).toBeFalse();
  });

  it('getTelegramLink должен правильно собирать URL для перехода в бота', () => {
    component.tgStatus = { isLinked: false, botUsername: 'INestBot', verificationToken: 'token123' };
    expect(component.getTelegramLink()).toBe('https://t.me/INestBot?start=token123');
  });
});