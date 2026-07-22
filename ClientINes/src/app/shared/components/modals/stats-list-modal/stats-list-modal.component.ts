import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { StorageLocation } from '../../../../core/contracts/storage-location';
import { DashboardFacade } from '../../../../features/dashboard/dashboard.facade';
import { StatsListType } from '../../../../features/dashboard/dashboard.modal.service';
import { ItemStatus } from '../../../../core/enums/item-status.enum';
import { ReminderType } from '../../../../core/enums/reminder-type.enum';

export type AttentionSeverity = 'danger' | 'warning' | 'info';

export interface AttentionItemViewModel {
  itemId: string;
  itemName: string;
  locationName: string;
  typeKey: string;
  date: Date;
  severity: AttentionSeverity;
}

@Component({
  selector: 'app-stats-list-modal',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './stats-list-modal.component.html',
  styleUrl: './stats-list-modal.component.scss'
})
export class StatsListModalComponent {
  @Input() type: StatsListType | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() selectLocation = new EventEmitter<string>();

  public facade = inject(DashboardFacade);
  private router = inject(Router);

  get modalTitleKey(): string {
    switch (this.type) {
      case 'locations': return 'DASHBOARD_STATS.LOCATIONS';
      case 'lent': return 'DASHBOARD_STATS.LENT';
      case 'attention': return 'DASHBOARD_STATS.ATTENTION';
      default: return '';
    }
  }

  get locationsList(): StorageLocation[] {
    return this.facade.locations.flatLocations;
  }

  get lentItems() {
    return this.facade.locations.flatLocations
      .flatMap(l => (l.items || []).map(i => ({ item: i, locationName: l.name })))
      .filter(x => 
        x.item.status === ItemStatus.Lent || 
        x.item.status === ItemStatus.Borrowed || 
        !!x.item.lending
      );
  }

  get attentionItems(): AttentionItemViewModel[] {
    const result: AttentionItemViewModel[] = [];
    const now = new Date();
    const warningThreshold = new Date();
    warningThreshold.setDate(now.getDate() + 3);

    this.facade.locations.flatLocations.forEach(loc => {
      (loc.items || []).forEach(item => {

        if (item.reminders && item.reminders.length > 0) {
          item.reminders.forEach(r => {
            if (r.triggerAt && !r.isCompleted) {
              const remDate = new Date(r.triggerAt);
              if (!isNaN(remDate.getTime())) {
                
                let typeKey = 'DASHBOARD_STATS.ATTENTION';
                if (r.type === ReminderType.ReturnItem) {
                  typeKey = 'DASHBOARD_STATS.LENT';
                } else if (r.type === ReminderType.Warranty) {
                  typeKey = 'DASHBOARD_STATS.WARRANTY_SHORT';
                }

                let severity: AttentionSeverity = 'info';

                if (remDate < now) {
                  severity = 'danger';
                } else if (remDate <= warningThreshold) {
                  severity = 'warning';
                }

                result.push({
                  itemId: item.id,
                  itemName: item.name,
                  locationName: loc.name,
                  typeKey,
                  date: remDate,
                  severity
                });
              }
            }
          });
        }

        const wExpiration = item.details?.warrantyExpiration;
        if (wExpiration) {
          const wDate = new Date(wExpiration);
          if (!isNaN(wDate.getTime())) {
            let severity: AttentionSeverity = 'info';
            if (wDate < now) severity = 'danger';
            else if (wDate <= warningThreshold) severity = 'warning';

            result.push({
              itemId: item.id,
              itemName: item.name,
              locationName: loc.name,
              typeKey: 'DASHBOARD_STATS.WARRANTY_SHORT',
              date: wDate,
              severity
            });
          }
        }

      });
    });

    return result.sort((a, b) => a.date.getTime() - b.date.getTime());
  }

  onLocationClick(locId: string) {
    this.router.navigate(['/location', locId]);
    this.close.emit();
  }

  onItemClick(itemId: string) {
    this.router.navigate(['/item', itemId]);
    this.close.emit();
  }
}