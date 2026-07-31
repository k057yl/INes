import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { StorageLocation } from '../../../../core/contracts/storage-location';
import { DashboardFacade } from '../../../../features/dashboard/dashboard.facade';
import { StatsListType } from '../../../../features/dashboard/dashboard.modal.service';
import { ItemStatus } from '../../../../core/enums/item-status.enum';
import { AttentionItemDto } from '../../../../core/dtos/attention-item.dto';

export type AttentionSeverity = 'danger' | 'warning' | 'info';

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

  get attentionItems(): AttentionItemDto[] {
    return this.facade.stats?.attentionItems || [];
  }

  get upcomingAttentionItems(): AttentionItemDto[] {
    return this.attentionItems.filter(i => i.severity !== 'danger');
  }

  get expiredAttentionItems(): AttentionItemDto[] {
    return this.attentionItems.filter(i => i.severity === 'danger');
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