import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgApexchartsModule } from 'ng-apexcharts';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/auth/auth.service';
import { DashboardStats } from '../../core/models/models';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { SentimentBadgeComponent } from '../../shared/components/sentiment-badge/sentiment-badge.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { DurationPipe } from '../../shared/pipes/duration.pipe';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';
import { areaChart, donutChart } from './dashboard.charts';

@Component({
  selector: 'ev-dashboard',
  standalone: true,
  imports: [
    RouterLink, NgApexchartsModule, StatCardComponent, SentimentBadgeComponent,
    StatusBadgeComponent, EmptyStateComponent, SkeletonComponent,
    DurationPipe, RelativeTimePipe, InitialsPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  private readonly dashboard = inject(DashboardService);
  private readonly auth = inject(AuthService);

  readonly loading = signal(true);
  readonly stats = signal<DashboardStats | null>(null);

  readonly greeting = computed(() => {
    const h = new Date().getHours();
    const name = this.auth.user()?.firstName ?? 'there';
    const part = h < 12 ? 'Good morning' : h < 17 ? 'Good afternoon' : 'Good evening';
    return `${part}, ${name}`;
  });

  readonly areaOptions = computed(() => areaChart(this.stats()?.callsThisWeek ?? []));
  readonly donutOptions = computed(() => donutChart(this.stats()?.outcomeBreakdown));

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.dashboard.getStats().subscribe({
      next: (s) => { this.stats.set(s); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  feesAmount(): string {
    const amt = this.stats()?.feesConfirmedAmount ?? 0;
    return `₹${amt.toLocaleString('en-IN')} confirmed`;
  }
}
