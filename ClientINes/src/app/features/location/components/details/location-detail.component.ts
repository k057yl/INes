import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, Location as NgLocation } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { environment } from '../../../../../environments/environment';
import { ItemStatus } from '../../../item/enums/item-status.enum';
import { ItemService } from '../../../item/services/item.service';
import { LocationService } from '../../services/location.service';
import { DashboardModalService } from '../../../dashboard/components/dashboard/dashboard.modal.service';

@Component({
  selector: 'app-location-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './location-detail.component.html',
  styleUrl: './location-detail.component.scss'
})
export class LocationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private itemService = inject(ItemService);
  public locationService = inject(LocationService);
  private ngLocation = inject(NgLocation);
  private modal = inject(DashboardModalService);
  
  private readonly baseUrl = environment.apiBaseUrl.replace('/api', '');
  private readonly googleColors = ['var(--g-blue)', 'var(--g-red)', 'var(--g-yellow)', 'var(--g-green)'];

  location: any = null;
  isLoading = true;
  breadcrumbs: any[] = [];

  readonly statusKeys: Record<number, string> = {
    [ItemStatus.Active]: 'STATUS.ACTIVE',
    [ItemStatus.Lent]: 'STATUS.LENT',
    [ItemStatus.Sold]: 'STATUS.SOLD',
    [ItemStatus.Archived]: 'STATUS.ARCHIVED'
  };

  ngOnInit() {
    this.route.data.subscribe({
      next: (data) => {
        this.location = data['locationData'];
        
        if (this.location) {
          this.buildBreadcrumbs(this.location);
        }
        
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  printQrCode() {
    const printZone = document.getElementById('location-qr-zone');
    if (!printZone) return;

    const windowPrint = window.open('', '', 'left=0,top=0,width=600,height=600,toolbar=0,scrollbars=0,status=0');
    if (!windowPrint) return;

    const qrImageEl = printZone.querySelector('img') as HTMLImageElement;
    const qrSrc = qrImageEl ? qrImageEl.src : '';

    windowPrint.document.write(`
      <html>
        <head>
          <title>Print QR Code - ${this.location?.name || ''}</title>
          <style>
            body { 
              margin: 0; display: flex; justify-content: center; align-items: center; 
              height: 100vh; font-family: sans-serif; background: #fff; color: #000;
            }
            .qr-wrapper-card { text-align: center; border: 2px solid #000; padding: 24px; border-radius: 12px; }
            .qr-card-title { font-size: 1.2rem; font-weight: bold; margin-bottom: 12px; text-transform: uppercase; }
            .qr-image { width: 220px; height: 220px; display: block; margin: 0 auto; }
            .qr-card-footer { font-size: 0.75rem; color: #444; margin-top: 12px; letter-spacing: 1px; font-weight: bold; }
          </style>
        </head>
        <body>
          <div class="qr-wrapper-card">
            <div class="qr-card-title">${this.location?.name || ''}</div>
            <img src="${qrSrc}" class="qr-image" alt="QR Code" id="printable-qr">
            <div class="qr-card-footer">МЕСТО ХРАНЕНИЯ</div>
          </div>
          <script>
            const img = document.getElementById('printable-qr');
            if (img.complete) {
              window.print();
              window.close();
            } else {
              img.onload = () => {
                window.print();
                window.close();
              };
            }
          </script>
        </body>
      </html>
    `);
    windowPrint.document.close();
    windowPrint.focus();
  }

  private buildBreadcrumbs(current: any) {
    const path: any[] = [];
    let temp: any = current;
    
    while (temp) {
      path.unshift(temp);
      temp = temp.parentLocation;
    }
    this.breadcrumbs = path;
  }

  onEditItem(item: any) {
    this.modal.openItemForm(item).subscribe(res => {
      if (res && this.location) {
        const index = this.location.items.findIndex((i: any) => i.id === item.id);
        if (index !== -1) {
          this.location.items[index] = { ...this.location.items[index], ...res };
          this.location = { ...this.location };
        }
      }
    });
  }

  onDeleteItem(item: any) { 
    this.modal.openConfirm({
      mode: 'delete',
      title: 'COMMON.DELETE',
      message: 'ITEM_CARD.MODAL.YOU_SURE_MSG'
    }).subscribe((res: any) => {
      if (res) {
        this.itemService.archive(item.id).subscribe({
          next: () => {
            if (this.location) {
              this.location = {
                ...this.location,
                items: this.location.items.filter((i: any) => i.id !== item.id)
              };
            }
          }
        });
      }
    });
  }

  getAccentColor(id: string): string {
    const sum = id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return this.googleColors[sum % this.googleColors.length];
  }

  getPhotoUrl(path: string | null | undefined): string {
    if (!path) return '';
    return path.startsWith('http') ? path : `${this.baseUrl}/${path}`;
  }

  getStatusKey(status: number): string {
    return this.statusKeys[status] || 'STATUS.ACTIVE';
  }

  getStatusClass(status: number): string {
    return ItemStatus[status]?.toLowerCase() || 'active';
  }

  goBack(): void {
    this.router.navigate(['/dashboard']); // <- Ведёт только на дашборд
  }
}