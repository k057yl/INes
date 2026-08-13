import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Item } from '../../../item/contracts/item';
import { SaleCreateDto } from '../../dtos/sale-item-create.dto';
import { Platform } from '../../../platform/contracts/platform';
import { SalesService } from '../../services/sales.service';
import { InestModalComponent } from '../../../../shared/components/inest-modal/inest-modal.component';
import { AuthService } from '../../../auth/services/auth.service';
import { TutorialService, TutorialStep } from '../../../../core/services/tutorial.service';
import { FORM_VALIDATION } from '../../../../shared/constants/form-defaults.constants';

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
  private toastr = inject(ToastrService);
  private authService = inject(AuthService);
  private tutorialService = inject(TutorialService);

  @Input() item!: Item;
  platforms: Platform[] = [];
  showPlatformModal = false;
  
  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<SaleCreateDto>();

  private readonly localToday = this.getLocalDateString();

  sellForm = this.fb.group({
    salePrice: [null as number | null, [Validators.required, Validators.min(FORM_VALIDATION.PRICE.MIN)]],
    soldDate: [this.localToday, [Validators.required, this.futureDateValidator()]],
    platformId: [null as string | null, [Validators.required]],
    comment: ['']
  });

  get purchasePriceDisplay(): string {
    if (this.item?.details?.purchasePrice != null) {
      const currency = this.item.details.currency || 'USD';
      return `${this.item.details.purchasePrice} ${currency}`;
    }
    return '—';
  }

  ngOnInit(): void {
    this.loadPlatforms();
    this.checkAndStartTutorial();
  }

  private checkAndStartTutorial() {
    this.authService.user$.pipe(take(1)).subscribe(user => {
      if (!user) return;
      
      const isSellFormPassed = (user.completedTutorials & TutorialStep.SellForm) === TutorialStep.SellForm;
      if (!isSellFormPassed) {
        setTimeout(() => {
          this.tutorialService.startSellFormTour(() => {
            user.completedTutorials |= TutorialStep.SellForm;
            this.authService.updateLocalUserTutorial(TutorialStep.SellForm);
          });
        }, 300);
      }
    });
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
      error: () => this.toastr.error(this.translate.instant('PLATFORMS.ERRORS.NOT_FOUND'))
    });
  }

  addPlatform() {
    this.showPlatformModal = true;
  }

  onPlatformConfirmed(name: string) {
    this.salesService.addPlatform({ name }).subscribe({
      next: (res: any) => {
        this.toastr.success(this.translate.instant('PLATFORMS.SUCCESS.CREATE'));
        this.showPlatformModal = false;

        this.salesService.getPlatforms().subscribe(platforms => {
          this.platforms = platforms;

          const newPlatformId = res?.id || platforms.find((p: any) => p.name === name)?.id;
          if (newPlatformId) {
            this.sellForm.patchValue({ platformId: newPlatformId });
          }
        });
      },
      error: (err) => {
        this.showPlatformModal = false;
        this.toastr.error(this.translate.instant(err.error?.error || 'SYSTEM.DEFAULT_ERROR'));
      }
    });
  }

  onSubmit() {
    if (this.item.status === 1) {
      this.toastr.warning(this.translate.instant('STATUS.ERRORS.CANT_SELL_LENT'));
      this.close.emit();
      return;
    }

    if (this.item.status === 2) {
      this.toastr.warning(this.translate.instant('STATUS.ERRORS.ALREADY_SOLD'));
      this.close.emit();
      return;
    }

    if (this.sellForm.invalid) {
      this.sellForm.markAllAsTouched(); 
      return;
    }
    
    const formValue = this.sellForm.getRawValue();
    const dto: SaleCreateDto = {
      itemId: this.item.id,
      salePrice: Number(formValue.salePrice),
      currency: this.item.details?.currency || 'USD',
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