import { Component, ChangeDetectionStrategy, inject, signal, input, OnInit, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CampaignsService } from '../../core/services/campaigns.service';
import { ToastService } from '../../core/services/toast.service';
import { ConfirmService } from '../../shared/components/confirm-dialog/confirm.service';
import { Call, Campaign, PagedResult } from '../../core/models/models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { SentimentBadgeComponent } from '../../shared/components/sentiment-badge/sentiment-badge.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { DurationPipe } from '../../shared/pipes/duration.pipe';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';

@Component({
  selector: 'ev-campaign-detail',
  standalone: true,
  imports: [
    RouterLink, StatusBadgeComponent, SentimentBadgeComponent, SkeletonComponent,
    DurationPipe, RelativeTimePipe, InitialsPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a routerLink="/campaigns" class="ev-link small fw-semibold d-inline-block mb-3"><i class="bi bi-arrow-left me-1"></i>Back to campaigns</a>

    @if (loading() && !campaign()) {
      <div class="ev-card ev-card-pad"><ev-skeleton [rows]="5" [height]="24" /></div>
    } @else if (campaign(); as c) {
      <div class="ev-card ev-card-pad mb-3">
        <div class="d-flex justify-content-between align-items-start flex-wrap gap-3">
          <div>
            <div class="d-flex align-items-center gap-2 mb-1">
              <h1 class="ev-page-title mb-0">{{ c.campaignName }}</h1>
              <ev-status-badge [value]="c.status" />
              @if (c.status === 'Running') { <span class="ev-dot ev-dot--live ms-1"></span> }
            </div>
            <p class="ev-page-sub mb-0">{{ c.campaignType }} · created {{ c.createdAt | evRelativeTime }}</p>
          </div>
          <div class="d-flex gap-2">
            @if (c.status === 'Running') { <button class="btn btn-soft" (click)="pause()"><i class="bi bi-pause-fill me-1"></i>Pause</button> }
            @if (c.status === 'Paused') { <button class="btn btn-primary" (click)="resume()"><i class="bi bi-play-fill me-1"></i>Resume</button> }
            @if (c.status === 'Running' || c.status === 'Paused') { <button class="btn btn-soft text-danger" (click)="cancel()"><i class="bi bi-x-circle me-1"></i>Cancel</button> }
          </div>
        </div>

        <div class="mt-3">
          <div class="d-flex justify-content-between small mb-1">
            <span class="text-secondary-ev">Progress</span>
            <span class="fw-bold">{{ c.callsCompleted }}/{{ c.totalStudents }} ({{ pct(c) }}%)</span>
          </div>
          <div class="ev-progress" style="height:10px"><div class="ev-progress__bar" [style.width.%]="pct(c)"></div></div>
        </div>
      </div>

      <div class="row g-3 mb-3">
        <div class="col-6 col-md-3"><div class="ev-card ev-card-pad text-center"><div class="ev-stat__value text-success">{{ c.callsCompleted }}</div><div class="ev-stat__label">Completed</div></div></div>
        <div class="col-6 col-md-3"><div class="ev-card ev-card-pad text-center"><div class="ev-stat__value text-warning">{{ c.callsNoAnswer }}</div><div class="ev-stat__label">No Answer</div></div></div>
        <div class="col-6 col-md-3"><div class="ev-card ev-card-pad text-center"><div class="ev-stat__value text-danger">{{ c.callsFailed }}</div><div class="ev-stat__label">Failed</div></div></div>
        <div class="col-6 col-md-3"><div class="ev-card ev-card-pad text-center"><div class="ev-stat__value">{{ c.callsInitiated }}</div><div class="ev-stat__label">Initiated</div></div></div>
      </div>

      <div class="ev-card">
        <div class="ev-card-head"><h3 class="ev-card-title">Calls in this campaign</h3></div>
        @if ((calls()?.items?.length ?? 0) === 0) {
          <div class="ev-card-pad text-center text-secondary-ev py-4">No calls placed yet.</div>
        } @else {
          <div class="ev-table-wrap">
            <table class="ev-table">
              <thead><tr><th>Student</th><th>Phone</th><th>Duration</th><th>Sentiment</th><th>Status</th><th>Time</th></tr></thead>
              <tbody>
                @for (call of calls()!.items; track call.id) {
                  <tr>
                    <td>
                      <div class="d-flex align-items-center gap-2">
                        <div class="ev-avatar ev-avatar--sm">{{ call.studentName | evInitials }}</div>
                        <span class="ev-cell-strong">{{ call.studentName }}</span>
                      </div>
                    </td>
                    <td class="ev-cell-sub">{{ call.parentPhone }}</td>
                    <td class="ev-cell-sub">{{ call.durationSeconds | evDuration }}</td>
                    <td><ev-sentiment-badge [value]="call.sentiment" /></td>
                    <td><ev-status-badge [value]="call.status" /></td>
                    <td class="ev-cell-sub">{{ call.createdAt | evRelativeTime }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>
    }
  `,
})
export class CampaignDetailComponent implements OnInit, OnDestroy {
  private readonly service = inject(CampaignsService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly id = input.required<string>();
  readonly loading = signal(true);
  readonly campaign = signal<Campaign | null>(null);
  readonly calls = signal<PagedResult<Call> | null>(null);

  private timer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.load();
    // Poll every 10s while running.
    this.timer = setInterval(() => {
      if (this.campaign()?.status === 'Running') this.load(true);
    }, 10000);
  }

  ngOnDestroy(): void { if (this.timer) clearInterval(this.timer); }

  load(silent = false): void {
    if (!silent) this.loading.set(true);
    this.service.get(this.id(), silent).subscribe({
      next: (data) => {
        const { calls, ...campaign } = data as Campaign & { calls: PagedResult<Call> };
        this.campaign.set(campaign as Campaign);
        this.calls.set(calls);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  pct(c: Campaign): number { return c.totalStudents ? Math.round((c.callsCompleted / c.totalStudents) * 100) : 0; }

  pause(): void { this.service.pause(this.id()).subscribe(() => { this.toast.info('Paused'); this.load(); }); }
  resume(): void { this.service.resume(this.id()).subscribe(() => { this.toast.success('Resumed'); this.load(); }); }

  async cancel(): Promise<void> {
    const ok = await this.confirm.ask({ title: 'Cancel campaign?', message: 'Remaining calls will not be placed.', danger: true, confirmText: 'Cancel campaign' });
    if (!ok) return;
    this.service.cancel(this.id()).subscribe(() => { this.toast.success('Campaign cancelled'); this.load(); });
  }
}
