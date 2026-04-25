import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';

import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Item } from '../../../models/entities/item.entity';
import { SellItemRequestDto } from '../../../models/dtos/sale.dto';
import { Platform } from '../../../models/entities/platform.entity';
import { SalesService } from '../../services/sales.service';
import { InestModalComponent } from '../modals/inest-modal/inest-modal.component';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { FORM_VALIDATION } from '../../constants/form-defaults.constants';

@Component({
  selector: 'app-sell-modal',
  standalone: true,
  imports: [ReactiveFormsModule, TranslateModule, InestModalComponent],
  templateUrl: './sell-modal.component.html',
  styleUrls: ['./sell-modal.component.scss']
})
export class SellModalComponent implements OnInit {
  private fb = inject(FormBuilder);
  private salesService = inject(SalesService);
  private translate = inject(TranslateService);

  @Input() item!: Item;
  platforms: Platform[] = [];
  showPlatformModal = false;
  
  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<SellItemRequestDto>();

  private readonly localToday = this.getLocalDateString();

  sellForm = this.fb.group({
    salePrice: [null as number | null, [Validators.required, Validators.min(FORM_VALIDATION.PRICE.MIN)]],
    soldDate: [this.localToday, [Validators.required, this.futureDateValidator()]],
    platformId: [null as string | null, [Validators.required]],
    comment: ['']
  });

  ngOnInit(): void {
    this.loadPlatforms();
  }

  private getLocalDateString(): string {
    const now = new Date();
    const pad = (num: number) => (num < 10 ? '0' : '') + num;
    return now.getFullYear() + '-' + pad(now.getMonth() + 1) + '-' + pad(now.getDate());
  }

  private futureDateValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      return control.value > this.getLocalDateString() ? { futureDate: true } : null;
    };
  }

  isControlInvalid(controlName: string, errorName: string): boolean {
    const control = this.sellForm.get(controlName);
    return !!(control?.touched && control?.hasError(errorName));
  }

  loadPlatforms() {
    this.salesService.getPlatforms().subscribe({
      next: (data) => this.platforms = data,
      error: (err) => console.error('Ошибка при загрузке платформ', err)
    });
  }

  addPlatform() {
    this.showPlatformModal = true;
  }

  onPlatformConfirmed(name: string) {
    this.salesService.addPlatform({ name }).subscribe({
      next: (newPlatform: Platform) => {
        this.platforms.push(newPlatform);
        this.sellForm.patchValue({ platformId: newPlatform.id });
        this.showPlatformModal = false;
      },
      error: (err) => {
        this.showPlatformModal = false;
        alert(this.translate.instant(err.error?.error || 'SYSTEM.DEFAULT_ERROR'));
      }
    });
  }

  onSubmit() {
    if (this.item.status === 1 || this.item.status === 7) {
      alert(this.translate.instant('STATUS.ERRORS.CANT_SELL_LENT'));
      this.close.emit();
      return;
    }

    if (this.sellForm.invalid) {
      this.sellForm.markAllAsTouched(); 
      return;
    }
    
    const formValue = this.sellForm.getRawValue();
    const dto: SellItemRequestDto = {
      itemId: this.item.id,
      salePrice: Number(formValue.salePrice),
      soldDate: new Date(formValue.soldDate!).toISOString(),
      platformId: formValue.platformId!,
      comment: formValue.comment || undefined
    };
    this.confirm.emit(dto);
  }

  onCancel() {
    this.close.emit();
  }
}