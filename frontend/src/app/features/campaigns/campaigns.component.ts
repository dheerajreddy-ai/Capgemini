import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CampaignsService } from '../../core/services/campaigns.service';
import { Campaign } from '../../core/models/models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { CampaignWizardComponent } from './campaign-wizard.component';

@Component({
  selector: 'ev-campaigns',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, EmptyStateComponent, SkeletonComponent, RelativeTimePipe, CampaignWizardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Campaigns</h1>
        <p class="ev-page-sub">Launch automated parent call campaigns</p>
      </div>
      <button class="btn btn-primary" (click)="showWizard.set(true)"><i class="bi bi-plus-lg me-2"></i>New Campaign</button>
    </div>

    @if (loading()) {
      <div class="row g-3">
        @for (i of [1,2,3]; track i) { <div class="col-md-6 col-xl-4"><div class="ev-card ev-card-pad"><ev-skeleton [rows]="4" [height]="20" /></div></div> }
      </div>
    } @else if (campaigns().length === 0) {
      <div class="ev-card"><ev-empty-state icon="bi-megaphone" title="No campaigns yet"
        message="Create your first campaign to start reaching parents automatically." /></div>
    } @else {
      <div class="row g-3">
        @for (c of campaigns(); track c.id) {
          <div class="col-md-6 col-xl-4">
            <a [routerLink]="['/campaigns', c.id]" class="ev-card ev-card--hover ev-card-pad h-100 d-block text-decoration-none text-reset">
              <div class="d-flex justify-content-between align-items-start mb-2">
                <div class="ev-campaign-ico" [class]="'ev-campaign-ico--' + (c.campaignType === 'FeeReminder' ? 'fee' : 'progress')">
                  <i class="bi" [class.bi-cash-coin]="c.campaignType === 'FeeReminder'" [class.bi-graph-up]="c.campaignType !== 'FeeReminder'"></i>
                </div>
                <ev-status-badge [value]="c.status" />
              </div>
              <h5 class="mb-1">{{ c.campaignName }}</h5>
              <div class="text-secondary-ev small mb-3">{{ c.campaignType }} · {{ c.createdAt | evRelativeTime }}</div>

              <div class="d-flex justify-content-between small mb-1">
                <span class="text-secondary-ev">Progress</span>
                <span class="fw-bold">{{ c.callsCompleted }}/{{ c.totalStudents }}</span>
              </div>
              <div class="ev-progress"><div class="ev-progress__bar" [style.width.%]="pct(c)"></div></div>

              <div class="ev-campaign-stats">
                <div><b class="text-success">{{ c.callsCompleted }}</b><span>Done</span></div>
                <div><b class="text-warning">{{ c.callsNoAnswer }}</b><span>No answer</span></div>
                <div><b class="text-danger">{{ c.callsFailed }}</b><span>Failed</span></div>
              </div>
            </a>
          </div>
        }
      </div>
    }

    <ev-campaign-wizard [open]="showWizard()" (close)="showWizard.set(false)" (created)="onCreated()" />
  `,
  styles: [`
    .ev-campaign-ico { width: 46px; height: 46px; border-radius: 13px; display: grid; place-items: center; font-size: 1.25rem; color: #fff; }
    .ev-campaign-ico--fee { background: var(--ev-grad-success); }
    .ev-campaign-ico--progress { background: var(--ev-grad-info); }
    .ev-campaign-stats { display: flex; gap: 1rem; margin-top: 1rem; padding-top: 1rem; border-top: 1px solid var(--ev-border); }
    .ev-campaign-stats b { font-family: "Sora",sans-serif; font-size: 1.15rem; display: block; line-height: 1; }
    .ev-campaign-stats span { font-size: .72rem; color: var(--ev-text-muted); }
  `],
})
export class CampaignsComponent implements OnInit {
  private readonly service = inject(CampaignsService);

  readonly loading = signal(true);
  readonly campaigns = signal<Campaign[]>([]);
  readonly showWizard = signal(false);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.list().subscribe({
      next: (c) => { this.campaigns.set(c); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  onCreated(): void { this.showWizard.set(false); this.load(); }

  pct(c: Campaign): number {
    return c.totalStudents ? Math.round((c.callsCompleted / c.totalStudents) * 100) : 0;
  }
}
