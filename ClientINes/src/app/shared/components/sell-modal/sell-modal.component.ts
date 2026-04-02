import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Item } from '../../../models/entities/item.entity';
import { SellItemRequestDto } from '../../../models/dtos/sale.dto';
import { Platform } from '../../../models/entities/platform.entity';
import { SalesService } from '../../services/sales.service';
import { InestModalComponent } from '../modal/shared-modal/inest-modal.component';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { FORM_VALIDATION } from '../../constants/form-defaults.constants';

@Component({
  selector: 'app-sell-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, InestModalComponent],
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

  sellForm = this.fb.group({
    salePrice: [null as number | null, [Validators.required, Validators.min(FORM_VALIDATION.PRICE.MIN)]],
    soldDate: [FORM_VALIDATION.DATE.TODAY, [Validators.required, this.futureDateValidator()]],
    platformId: [null as string | null],
    comment: ['']
  });

  private getTodayDate(): string {
    return new Date().toISOString().substring(0, 10);
  }

  private futureDateValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const dateValue = new Date(control.value);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      return dateValue > today ? { futureDate: true } : null;
    };
  }

  isControlInvalid(controlName: string, errorName: string): boolean {
    const control = this.sellForm.get(controlName);
    return !!(control?.touched && control?.hasError(errorName));
  }

  ngOnInit(): void {
    this.loadPlatforms();
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
    if (this.sellForm.invalid) {
      this.sellForm.markAllAsTouched(); 
      return;
    }
    if (this.sellForm.valid) {
      const formValue = this.sellForm.getRawValue();
      const dto: SellItemRequestDto = {
        itemId: this.item.id,
        salePrice: Number(formValue.salePrice),
        soldDate: new Date(formValue.soldDate!).toISOString(),
        platformId: formValue.platformId || null,
        comment: formValue.comment || undefined
      };
      this.confirm.emit(dto);
    }
  }

  onCancel() {
    this.close.emit();
  }
}