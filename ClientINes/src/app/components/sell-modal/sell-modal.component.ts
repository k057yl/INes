import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Item } from '../../models/entities/item.entity';
import { SellItemRequestDto } from '../../models/dtos/sale.dto';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-sell-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './sell-modal.component.html',
  styleUrls: ['./sell-modal.component.css']
})
export class SellModalComponent implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);

  @Input() item!: Item;
  platforms: any[] = []; 
  
  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<SellItemRequestDto>();

  sellForm = this.fb.group({
    salePrice: [0, [Validators.required, Validators.min(0.01)]],
    soldDate: [new Date().toISOString().substring(0, 10), Validators.required],
    platformId: [null as string | null],
    comment: ['']
  });

  ngOnInit(): void {
    this.loadPlatforms();
  }

  loadPlatforms() {
    this.http.get<any[]>(`${environment.apiBaseUrl}/platforms`).subscribe({
      next: (data) => this.platforms = data,
      error: (err) => console.error('Ошибка при загрузке платформ', err)
    });
  }

  addPlatform() {
    const name = prompt('Введите название платформы (например: Avito, eBay):');
    if (name && name.trim()) {
      this.http.post<any>(`${environment.apiBaseUrl}/platforms`, JSON.stringify(name.trim()), {
        headers: { 'Content-Type': 'application/json' }
      }).subscribe({
        next: (newPlatform) => {
          this.platforms.push(newPlatform);
          this.sellForm.patchValue({ platformId: newPlatform.id });
        },
        error: (err) => alert('Не удалось создать платформу')
      });
    }
  }

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