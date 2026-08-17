import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InestModalComponent } from './inest-modal.component';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';

describe('InestModalComponent', () => {
  let component: InestModalComponent;
  let fixture: ComponentFixture<InestModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        InestModalComponent,
        FormsModule,
        TranslateModule.forRoot()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InestModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('должен создаваться', () => {
    expect(component).toBeTruthy();
  });

  it('getIcon должен возвращать правильные иконки для каждого режима', () => {
    component.mode = 'delete';
    expect(component.getIcon()).toBe('fa-exclamation-triangle');

    component.mode = 'confirm';
    expect(component.getIcon()).toBe('fa-undo-alt');

    component.mode = 'input';
    expect(component.getIcon()).toBe('fa-edit');
  });

  it('getButtonClass должен возвращать соответствующий CSS-класс', () => {
    component.mode = 'delete';
    expect(component.getButtonClass()).toBe('inest-btn-danger');

    component.mode = 'confirm';
    expect(component.getButtonClass()).toBe('inest-btn-confirm');

    component.mode = 'input';
    expect(component.getButtonClass()).toBe('inest-btn-primary');
  });

  it('getConfirmText должен правильно определять текст кнопки', () => {
    component.confirmText = 'COMMON.SAVE';

    component.mode = 'delete';
    expect(component.getConfirmText()).toBe('COMMON.DELETE');

    component.mode = 'confirm';
    expect(component.getConfirmText()).toBe('COMMON.OK');

    component.mode = 'input';
    expect(component.getConfirmText()).toBe('COMMON.SAVE');

    component.confirmText = 'CUSTOM_TEXT';
    expect(component.getConfirmText()).toBe('CUSTOM_TEXT');
  });

  it('submit не должен эмитить данные при пустом имени в режиме input', () => {
    spyOn(component.confirmed, 'emit');

    component.mode = 'input';
    component.name = '   ';
    component.submit();

    expect(component.confirmed.emit).not.toHaveBeenCalled();
  });

  it('submit должен эмитить введенное значение в режиме input', () => {
    spyOn(component.confirmed, 'emit');

    component.mode = 'input';
    component.name = 'Новая Локация';
    component.submit();

    expect(component.confirmed.emit).toHaveBeenCalledWith('Новая Локация');
  });

  it('close должен эмитить событие отмены', () => {
    spyOn(component.cancelled, 'emit');

    component.close();

    expect(component.cancelled.emit).toHaveBeenCalled();
  });
});