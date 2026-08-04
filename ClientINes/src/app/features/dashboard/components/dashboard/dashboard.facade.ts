import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';
import { DashboardLocationService } from '../../services/dashboard-location.service';
import { DashboardItemService } from '../../services/dashboard-item.service';
import { DashboardNavigationService } from '../../services/dashboard-navigation.service';
import { DashboardActionExecutor } from '../../services/dashboard-action-executor.service';
import { DashboardTreeService } from '../../services/dashboard-tree.service';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardStatsDto } from '../../dtos/dashboard-stats.dto';

@Injectable()
export class DashboardFacade {
  public locations = inject(DashboardLocationService);
  public items = inject(DashboardItemService);
  public nav = inject(DashboardNavigationService);
  public executor = inject(DashboardActionExecutor);
  public tree = inject(DashboardTreeService);
  private dashboardApi = inject(DashboardService);

  stats: DashboardStatsDto | null = null;
  isLoading = true;

  loadData(): Observable<any> {
    this.isLoading = true;

    return forkJoin({
      tree: this.locations.loadTree(),
      stats: this.dashboardApi.getStats()
    }).pipe(
      tap({
        next: (res) => {
          this.stats = res.stats;
          this.isLoading = false;
        },
        error: () => (this.isLoading = false)
      })
    );
  }

  get visibleConnectedLists(): string[] {
    const paged = this.nav.getBoardPageLocations(this.locations.locations);
    return this.locations.flatLocations
      .filter(l => paged.some(p => p.id === l.id || this.tree.isChildOf(l.id, p)))
      .map(l => l.id);
  }

  get visibleConnectedLocationLists(): string[] {
    return this.visibleConnectedLists.map(id => 'list-loc-' + id);
  }
}