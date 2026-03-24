import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { Item } from '../../../../models/entities/item.entity';
import { TranslateModule } from '@ngx-translate/core';
import { StatusNamePipe } from '../../../../shared/components/pipe/status-name.pipe';
import { ItemRemindersComponent } from '../reminder/item-reminders.component';

@Component({
  selector: 'app-item-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, StatusNamePipe, ItemRemindersComponent],
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

  historyIcons: { [key: number]: string } = {
    0: 'fa-plus-circle',    // Created
    1: 'fa-exchange-alt',   // Moved
    2: 'fa-info-circle',    // StatusChanged
    3: 'fa-tools',          // Repaired
    4: 'fa-handshake',      // Lent
    5: 'fa-undo',           // Returned
    6: 'fa-chart-line',     // ValueUpdated
    7: 'fa-bell'            // ReminderCompleted
  };

  get isLent(): boolean {
    return this.item?.status === 1;
  }

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
        this.item = data;
        this.activePhotoUrl = data.photoUrl || (data.photos?.length ? data.photos[0].filePath : null);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Ошибка загрузки предмета:', err);
        this.isLoading = false;
        if (err.status === 404) {
          this.router.navigate(['/main'], { replaceUrl: true });
        }
      }
    });
  }

  getPhotoUrl(path: string | null | undefined): string {
    if (!path) return 'assets/images/no-image.png';
    if (path.startsWith('http')) return path;
    return `${this.baseUrl}/${path}`;
  }

  setMainPhoto(path: string) {
    this.activePhotoUrl = path;
  }

  goBack() {
    if (window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigate(['/main']);
    }
  }
}