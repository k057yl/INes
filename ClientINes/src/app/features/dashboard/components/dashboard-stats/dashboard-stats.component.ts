import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { RouterLink } from '@angular/router';
import { DashboardStatsDto } from '../../dtos/dashboard-stats.dto';
import { DashboardModalService, StatsListType } from '../dashboard/dashboard.modal.service';

@Component({
  selector: 'app-dashboard-stats',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule],
  templateUrl: './dashboard-stats.component.html',
  styleUrl: './dashboard-stats.component.scss'
})
export class DashboardStatsComponent {
  @Input() stats: DashboardStatsDto | null = null;

  public modal = inject(DashboardModalService);

  openStats(type: StatsListType) {
    this.modal.openStatsList(type);
  }

  openCreateLocation() {
    this.modal.openLocationForm();
  }

  openCreateItem() {
    this.modal.openItemForm();
  }
}