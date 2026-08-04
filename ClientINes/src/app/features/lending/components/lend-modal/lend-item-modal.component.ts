import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

import { Item } from '../../../item/contracts/item';
import { ItemLendDto } from '../../../item/dtos/item-lend.dto';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-lend-item-modal',
  standalone: true,
  imports: [ReactiveFormsModule, TranslateModule],
  templateUrl: './lend-item-modal.component.html',
  styleUrls: ['./lend-item-modal.component.scss']
})
export class LendItemModalComponent implements OnInit {
  @Input() item!: Item;
  @Input() isOpen = false;
  
  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<ItemLendDto>();

  lendForm!: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.lendForm = this.fb.group({
      personName: ['', [Validators.required, Validators.maxLength(50)]],
      valueAtLending: [this.item?.details?.estimatedValue || 0, [Validators.min(0)]],
      expectedReturnDate: [null],
      comment: ['', [Validators.maxLength(200)]]
    });
  }

  onSubmit(): void {
    if (this.lendForm.valid) {
      const formValue = this.lendForm.value;
      
      const dto: ItemLendDto = {
        itemId: this.item.id,
        personName: formValue.personName,
        valueAtLending: formValue.valueAtLending, 
        expectedReturnDate: formValue.expectedReturnDate || null,
        comment: formValue.comment || null,
        contactEmail: null, 
        sendNotification: false,
        direction: 0
      };

      this.confirm.emit(dto);
      this.lendForm.reset();
    } else {
      this.lendForm.markAllAsTouched();
    }
  }

  onCancel(): void {
    this.lendForm.reset();
    this.close.emit();
  }
}