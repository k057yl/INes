import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { Item } from '../../../../core/contracts/item';
import { ItemHistoryType } from '../../../../core/enums/item-history-type.enum';
import { TranslateModule } from '@ngx-translate/core';
import { StatusNamePipe } from '../../../../shared/pipes/status-name.pipe';
import { ItemRemindersComponent } from '../reminder/item-reminders.component';
import { PricePipe } from '../../../../shared/pipes/price-currency.pipe';
import { DashboardModalService } from '../../../dashboard/dashboard.modal.service';
import { ItemService } from '../../../../core/services/item.service';
import { LendingService } from '../../../../core/services/lending.service';

@Component({
  selector: 'app-item-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, StatusNamePipe, ItemRemindersComponent, PricePipe],
  templateUrl: './item-detail.component.html',
  styleUrls: ['./item-detail.component.scss']
})
export class ItemDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private modalService = inject(DashboardModalService);
  private itemService = inject(ItemService);
  private lendingService = inject(LendingService);

  item: Item | null = null;
  isLoading = true;
  activePhotoUrl: string | null = null;
  readonly baseUrl = environment.apiBaseUrl.replace('/api', '');

  showReminders = false;

  private readonly googleColors = [
    'var(--g-blue)', 
    'var(--g-red)', 
    'var(--g-yellow)', 
    'var(--g-green)'
  ];

  historyIcons: Record<number, string> = {
    [ItemHistoryType.Created]: 'fa-plus-circle',
    [ItemHistoryType.Moved]: 'fa-exchange-alt',
    [ItemHistoryType.Lent]: 'fa-handshake',
    [ItemHistoryType.Returned]: 'fa-undo',
    [ItemHistoryType.Sold]: 'fa-dollar-sign',
    [ItemHistoryType.ValueUpdated]: 'fa-chart-line',
    [ItemHistoryType.ReminderCompleted]: 'fa-check-double',
    [ItemHistoryType.ReminderScheduled]: 'fa-bell',
    [ItemHistoryType.Archived]: 'fa-archive'
  };

  get isLent(): boolean { return this.item?.status === 1; }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadItem(id);
    } else {
      this.router.navigate(['/main']);
    }
  }

  toggleReminders() {
    this.showReminders = !this.showReminders;
  }

  getAccentColor(): string {
    if (this.item?.storageLocation?.color) {
      return this.item.storageLocation.color;
    }

    if (!this.item) return this.googleColors[0];
    const sum = this.item.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return this.googleColors[sum % this.googleColors.length];
  }

  loadItem(id: string) {
    this.isLoading = true;
    this.http.get<Item>(`${environment.apiBaseUrl}/items/${id}`).subscribe({
      next: (data) => {
        if (data.history) {
          data.history.sort((a, b) => 
            new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
          );
        }
        
        this.item = data;
        this.activePhotoUrl = data.photoUrl || (data.photos?.length ? data.photos[0].filePath : null);
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 404) {
          this.router.navigate(['/dashboard'], { replaceUrl: true });
        }
      }
    });
  }

  onEdit() {
    if (!this.item) return;
    
    this.modalService.openItemForm(this.item).subscribe(res => {
      if (this.item) {
        this.loadItem(this.item.id);
      }
    });
  }

  getPhotoUrl(path: string | null | undefined): string {
    if (!path) return 'assets/images/no-image.png';
    return path.startsWith('http') ? path : `${this.baseUrl}/${path}`;
  }

  setMainPhoto(path: string) {
    this.activePhotoUrl = path;
  }

  getReceiptUrl(details: any): string | null {
    if (!details) return null;
    const path = details.receiptDocumentPath || details.receiptUrl || details.receiptPath;
    if (!path) return null;
    return path.startsWith('http') ? path : `${this.baseUrl}/${path}`;
  }

  goBack() {
    if (window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigate(['/dashboard']);
    }
  }

  onReturn() {
    if (!this.item) return;

    const message = 'LENDING_MODAL.MODAL.RETURN_MSG';

    this.modalService.openConfirm({
      mode: 'confirm',
      title: 'COMMON.RETURN',
      message: message,
      confirmText: 'COMMON.YES'
    }).subscribe((res) => {
      if (!res) return;

      this.lendingService.returnItem(this.item!.id, { returnedDate: new Date().toISOString() }).subscribe({
        next: () => {
          this.loadItem(this.item!.id);
        },
        error: (err) => console.error('Return failed', err)
      });

    });
  }
}