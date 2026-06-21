import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { NgApexchartsModule } from 'ng-apexcharts';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardStats } from '../../core/models/models';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { barChart, sentimentTrend, typePie } from './analytics.charts';

@Component({
  selector: 'ev-analytics',
  standalone: true,
  imports: [NgApexchartsModule, StatCardComponent, SkeletonComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Analytics</h1>
        <p class="ev-page-sub">Insights across your parent outreach</p>
      </div>
      <div class="btn-group ev-range">
        @for (r of ranges; track r) {
          <button class="btn" [class.btn-primary]="range() === r" [class.btn-soft]="range() !== r" (click)="range.set(r)">{{ r }}</button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="ev-card ev-card-pad"><ev-skeleton [rows]="8" [height]="28" /></div>
    } @else if (stats(); as s) {
      <!-- Summary -->
      <div class="row g-3 mb-3">
        <div class="col-6 col-xl-3"><ev-stat-card icon="bi-telephone" tone="primary" [value]="s.weeksCalls" label="Total Calls" /></div>
        <div class="col-6 col-xl-3"><ev-stat-card icon="bi-cash-stack" tone="success" [value]="'₹' + s.feesConfirmedAmount.toLocaleString('en-IN')" label="Fees Confirmed" /></div>
        <div class="col-6 col-xl-3"><ev-stat-card icon="bi-chat-square-dots" tone="danger" [value]="s.complaintsNew" label="Complaints" /></div>
        <div class="col-6 col-xl-3"><ev-stat-card icon="bi-emoji-smile" tone="info" [value]="positivePct(s) + '%'" label="Positive Sentiment" /></div>
      </div>

      <div class="row g-3 mb-3">
        <div class="col-12 col-lg-8">
          <div class="ev-card h-100">
            <div class="ev-card-head"><h3 class="ev-card-title">Calls by Day</h3></div>
            <div class="ev-card-pad">
              <apx-chart [series]="bar().series!" [chart]="bar().chart!" [colors]="bar().colors!"
                [plotOptions]="bar().plotOptions!" [dataLabels]="bar().dataLabels!" [xaxis]="bar().xaxis!"
                [grid]="bar().grid!" [yaxis]="bar().yaxis!" [legend]="bar().legend!" />
            </div>
          </div>
        </div>
        <div class="col-12 col-lg-4">
          <div class="ev-card h-100">
            <div class="ev-card-head"><h3 class="ev-card-title">Sentiment Mix</h3></div>
            <div class="ev-card-pad">
              <apx-chart [series]="pie().series!" [chart]="pie().chart!" [labels]="pie().labels!"
                [colors]="pie().colors!" [legend]="pie().legend!" [dataLabels]="pie().dataLabels!"
                [stroke]="pie().stroke!" [plotOptions]="pie().plotOptions!" />
            </div>
          </div>
        </div>
      </div>

      <div class="ev-card">
        <div class="ev-card-head"><h3 class="ev-card-title">Sentiment Trend</h3></div>
        <div class="ev-card-pad">
          <apx-chart [series]="trend().series!" [chart]="trend().chart!" [colors]="trend().colors!"
            [stroke]="trend().stroke!" [xaxis]="trend().xaxis!" [grid]="trend().grid!"
            [dataLabels]="trend().dataLabels!" [yaxis]="trend().yaxis!" [legend]="trend().legend!" />
        </div>
      </div>
    }
  `,
  styles: [`
    .ev-range .btn { border-radius: 10px; }
    .ev-range .btn:not(:last-child) { margin-right: .35rem; }
    apx-chart { display: block; }
  `],
})
export class AnalyticsComponent {
  private readonly dashboard = inject(DashboardService);

  readonly loading = signal(true);
  readonly stats = signal<DashboardStats | null>(null);
  readonly range = signal<'This Week' | 'This Month' | 'Last 3 Months'>('This Week');
  readonly ranges = ['This Week', 'This Month', 'Last 3 Months'] as const;

  readonly bar = computed(() => barChart(this.stats()?.callsThisWeek ?? []));
  readonly pie = computed(() => typePie(this.stats()?.sentimentBreakdown));
  readonly trend = computed(() => sentimentTrend(this.stats()?.callsThisWeek ?? []));

  constructor() {
    this.dashboard.getStats().subscribe({
      next: (s) => { this.stats.set(s); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  positivePct(s: DashboardStats): number {
    const b = s.sentimentBreakdown;
    const total = b.positive + b.neutral + b.negative || 1;
    return Math.round((b.positive / total) * 100);
  }
}
