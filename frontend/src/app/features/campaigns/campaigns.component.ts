import { Component, ChangeDetectionStrategy, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CampaignsService } from '../../core/services/campaigns.service';
import { CallingWindowStatus, Campaign } from '../../core/models/models';
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

    @if (window()) {
      <div class="alert mb-3 py-2 px-3" [class.alert-success]="window()!.isOpen" [class.alert-warning]="!window()!.isOpen">
        <i class="bi me-2" [class.bi-telephone-fill]="window()!.isOpen" [class.bi-clock]="!window()!.isOpen"></i>
        @if (window()!.isOpen) {
          <strong>Calling window is open</strong> — {{ window()!.window }}. Campaigns can run now.
        } @else {
          <strong>Outside calling hours</strong> ({{ window()!.window }}). Campaigns will be blocked until the window opens.
        }
      </div>
    }

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
            <div class="ev-card ev-card--hover ev-card-pad h-100">
              <div class="d-flex justify-content-between align-items-start mb-2">
                <div class="ev-campaign-ico" [class]="campaignIconClass(c)">
                  <i class="bi" [class.bi-cash-coin]="c.type === 'FeeReminder'"
                     [class.bi-graph-up]="c.type === 'ProgressUpdate'"
                     [class.bi-bell]="c.type === 'AttendanceAlert'"
                     [class.bi-chat-dots]="c.type === 'Custom'"></i>
                </div>
                <ev-status-badge [value]="c.status" />
              </div>
              <h5 class="mb-1"><a [routerLink]="['/campaigns', c.id]" class="text-decoration-none text-reset">{{ c.name }}</a></h5>
              <div class="text-secondary-ev small mb-3">{{ c.type }} · {{ c.createdAt | evRelativeTime }}</div>

              <div class="d-flex justify-content-between small mb-1">
                <span class="text-secondary-ev">Progress</span>
                <span class="fw-bold">{{ c.callsCompleted }}/{{ c.totalStudents }}</span>
              </div>
              <div class="ev-progress mb-3"><div class="ev-progress__bar" [style.width.%]="c.progressPercent"></div></div>

              <div class="ev-campaign-stats">
                <div><b class="text-success">{{ c.callsCompleted }}</b><span>Done</span></div>
                <div><b class="text-warning">{{ c.callsNoAnswer }}</b><span>No ans</span></div>
                <div><b class="text-danger">{{ c.callsFailed }}</b><span>Failed</span></div>
              </div>

              @if (c.status === 'Scheduled') {
                <div class="d-flex gap-2 mt-3">
                  <button class="btn btn-sm btn-primary flex-fill" [disabled]="!window()?.isOpen" (click)="startCampaign(c.id)">
                    <i class="bi bi-play-fill me-1"></i>Start Now
                  </button>
                  <button class="btn btn-sm btn-outline-danger" (click)="cancelCampaign(c.id)">Cancel</button>
                </div>
              }
            </div>
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
    .ev-campaign-ico--alert { background: var(--ev-grad-warning); }
    .ev-campaign-ico--custom { background: var(--ev-grad-primary); }
    .ev-campaign-stats { display: flex; gap: 1rem; padding-top: .75rem; border-top: 1px solid var(--ev-border); }
    .ev-campaign-stats b { font-family: "Sora",sans-serif; font-size: 1.1rem; display: block; line-height: 1; }
    .ev-campaign-stats span { font-size: .72rem; color: var(--ev-text-muted); }
  `],
})
export class CampaignsComponent implements OnInit, OnDestroy {
  private readonly service = inject(CampaignsService);
  private windowTimer?: ReturnType<typeof setInterval>;

  readonly loading = signal(true);
  readonly campaigns = signal<Campaign[]>([]);
  readonly showWizard = signal(false);
  readonly window = signal<CallingWindowStatus | null>(null);

  ngOnInit(): void {
    this.load();
    this.refreshWindow();
    this.windowTimer = setInterval(() => this.refreshWindow(), 60_000);
  }

  ngOnDestroy(): void { clearInterval(this.windowTimer); }

  load(): void {
    this.loading.set(true);
    this.service.list().subscribe({
      next: (r) => { this.campaigns.set(r.items ?? []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  refreshWindow(): void {
    this.service.getCallingWindow().subscribe({ next: (w) => this.window.set(w), error: () => {} });
  }

  onCreated(): void { this.showWizard.set(false); this.load(); }

  startCampaign(id: string): void {
    this.service.start(id).subscribe({ next: () => this.load(), error: () => {} });
  }

  cancelCampaign(id: string): void {
    this.service.cancel(id).subscribe({ next: () => this.load(), error: () => {} });
  }

  campaignIconClass(c: Campaign): string {
    const map: Record<string, string> = {
      FeeReminder: 'ev-campaign-ico--fee',
      ProgressUpdate: 'ev-campaign-ico--progress',
      AttendanceAlert: 'ev-campaign-ico--alert',
      Custom: 'ev-campaign-ico--custom',
    };
    return map[c.type] ?? 'ev-campaign-ico--custom';
  }
}
