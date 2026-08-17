import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LendItemModalComponent } from './lend-item-modal.component';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Item } from '../../../item/contracts/item';

describe('LendItemModalComponent', () => {
  let component: LendItemModalComponent;
  let fixture: ComponentFixture<LendItemModalComponent>;

  const mockItem = {
    id: 'item-777',
    name: 'Перфоратор',
    details: { purchasePrice: 200 }
  } as unknown as Item;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LendItemModalComponent, ReactiveFormsModule, TranslateModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(LendItemModalComponent);
    component = fixture.componentInstance;
    component.item = { ...mockItem };
    fixture.detectChanges();
  });

  it('должен подтягивать стоимость вещи по умолчанию при инициализации', () => {
    expect(component.lendForm.get('valueAtLending')?.value).toBe(200);
  });

  it('onSubmit не должен эмитить событие при пустом имени', () => {
    spyOn(component.confirm, 'emit');
    component.lendForm.patchValue({ personName: '' });

    component.onSubmit();

    expect(component.confirm.emit).not.toHaveBeenCalled();
    expect(component.lendForm.touched).toBeTrue();
  });

  it('onSubmit должен эмитить валидный ItemLendDto при заполнении', () => {
    spyOn(component.confirm, 'emit');

    component.lendForm.patchValue({
      personName: 'Сергей',
      valueAtLending: 200,
      comment: 'На пару дней'
    });

    component.onSubmit();

    expect(component.confirm.emit).toHaveBeenCalledWith(jasmine.objectContaining({
      itemId: 'item-777',
      personName: 'Сергей',
      valueAtLending: 200,
      comment: 'На пару дней'
    }));
  });

  it('onCancel должен сбрасывать форму и эмитить событие закрытия', () => {
    spyOn(component.close, 'emit');
    component.lendForm.patchValue({ personName: 'Сергей' });

    component.onCancel();

    expect(component.close.emit).toHaveBeenCalled();
    expect(component.lendForm.get('personName')?.value).toBeNull();
  });
});