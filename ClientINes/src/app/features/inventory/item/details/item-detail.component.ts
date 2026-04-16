import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { Item } from '../../../../models/entities/item.entity';
import { ItemHistoryType } from '../../../../models/enums/item-history-type.enum';
import { TranslateModule } from '@ngx-translate/core';
import { StatusNamePipe } from '../../../../shared/pipe/status-name.pipe';
import { ItemRemindersComponent } from '../reminder/item-reminders.component';
import { PricePipe } from '../../../../shared/pipe/price-currency.pipe';

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
    [ItemHistoryType.StatusChanged]: 'fa-sync-alt',
    [ItemHistoryType.Repaired]: 'fa-tools',
    [ItemHistoryType.Lent]: 'fa-handshake',
    [ItemHistoryType.Borrowed]: 'fa-reply-all',
    [ItemHistoryType.Returned]: 'fa-undo',
    [ItemHistoryType.ReturnedFromLend]: 'fa-home',
    [ItemHistoryType.ValueUpdated]: 'fa-chart-line',
    [ItemHistoryType.ReminderCompleted]: 'fa-check-double',
    [ItemHistoryType.ReminderScheduled]: 'fa-bell',
    [ItemHistoryType.Sold]: 'fa-dollar-sign'
  };

  get isLent(): boolean { return this.item?.status === 1; }
  get isBorrowed(): boolean { return this.item?.status === 7; }

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

  getPhotoUrl(path: string | null | undefined): string {
    if (!path) return 'assets/images/no-image.png';
    return path.startsWith('http') ? path : `${this.baseUrl}/${path}`;
  }

  setMainPhoto(path: string) {
    this.activePhotoUrl = path;
  }

  goBack() {
    if (window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigate(['/dashboard']);
    }
  }
}