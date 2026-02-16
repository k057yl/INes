import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Item } from '../../models/entities/item.entity';
import { SellItemRequestDto } from '../../models/dtos/sale.dto';
import { StorageLocation } from '../../models/entities/storage-location.entity';

@Component({
  selector: 'app-sell-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './sell-modal.component.html',
  styleUrls: ['./sell-modal.component.css']
})
export class SellModalComponent {
  private fb = inject(FormBuilder);

  @Input() item!: Item;
  @Input() platforms: StorageLocation[] = []; 
  
  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<SellItemRequestDto>();

  sellForm = this.fb.group({
    salePrice: [0, [Validators.required, Validators.min(0.01)]],
    soldDate: [new Date().toISOString().substring(0, 10), Validators.required],
    platformId: [''],
    comment: ['']
  });

  onSubmit() {
    if (this.sellForm.valid) {
      const formValue = this.sellForm.value;
      
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