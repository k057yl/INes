import { Component, Input, inject, signal, HostListener, ElementRef } from '@angular/core';
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
  private el = inject(ElementRef);

  isCreateMenuOpen = signal<boolean>(false);

  openStats(type: StatsListType) {
    this.modal.openStatsList(type);
  }

  toggleCreateMenu(event: MouseEvent) {
    event.stopPropagation();
    this.isCreateMenuOpen.set(!this.isCreateMenuOpen());
  }

  openCreateLocation() {
    this.modal.openLocationForm();
    this.isCreateMenuOpen.set(false);
  }

  openCreateItem() {
    this.modal.openItemForm();
    this.isCreateMenuOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    if (!this.el.nativeElement.contains(event.target)) {
      this.isCreateMenuOpen.set(false);
    }
  }
}