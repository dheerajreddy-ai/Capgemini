import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Call } from '../../core/models/models';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { SentimentBadgeComponent } from '../../shared/components/sentiment-badge/sentiment-badge.component';
import { DurationPipe } from '../../shared/pipes/duration.pipe';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';

@Component({
  selector: 'ev-call-detail-panel',
  standalone: true,
  imports: [DatePipe, DrawerComponent, SentimentBadgeComponent, DurationPipe, InitialsPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ev-drawer [open]="!!call()" size="lg" title="Call Details"
      [subtitle]="call() ? (call()!.studentName + ' · ' + call()!.studentClass) : ''" (close)="close.emit()">
      @if (call(); as c) {
        <!-- Info -->
        <div class="ev-call-info">
          <div class="d-flex align-items-center gap-3">
            <div class="ev-avatar ev-avatar--lg">{{ c.studentName | evInitials }}</div>
            <div class="flex-grow-1">
              <div class="fw-bold fs-5">{{ c.studentName }}</div>
              <div class="text-secondary-ev small">{{ c.parentName }} · {{ c.parentPhone }}</div>
            </div>
            <ev-sentiment-badge [value]="c.sentiment" />
          </div>
          <div class="ev-call-meta">
            <div><span>Date</span><b>{{ c.createdAt | date:'medium' }}</b></div>
            <div><span>Duration</span><b>{{ c.durationSeconds | evDuration }}</b></div>
            <div><span>Type</span><b>{{ c.callType }}</b></div>
            <div><span>Campaign</span><b>{{ c.campaignName ?? '—' }}</b></div>
          </div>
        </div>

        <!-- Outcomes -->
        <div class="ev-outcomes">
          <div class="ev-outcome" [class.is-on]="c.feesConfirmed"><i class="bi" [class.bi-check-circle-fill]="c.feesConfirmed" [class.bi-circle]="!c.feesConfirmed"></i>Fees Confirmed</div>
          <div class="ev-outcome" [class.is-warn]="c.hasComplaint"><i class="bi" [class.bi-exclamation-triangle-fill]="c.hasComplaint" [class.bi-circle]="!c.hasComplaint"></i>Complaint Filed</div>
          <div class="ev-outcome" [class.is-on]="c.callbackRequested"><i class="bi" [class.bi-arrow-repeat]="c.callbackRequested" [class.bi-circle]="!c.callbackRequested"></i>Callback Requested</div>
        </div>

        <!-- Recording -->
        @if (c.recordingUrl) {
          <div class="ev-section-label">Recording</div>
          <div class="ev-audio">
            <audio controls [src]="c.recordingUrl" class="w-100"></audio>
          </div>
        }

        <!-- AI Summary -->
        @if (c.summary) {
          <div class="ev-section-label">AI Summary</div>
          <div class="ev-summary"><i class="bi bi-stars me-2"></i>{{ c.summary }}</div>
        }

        <!-- Transcript -->
        <div class="ev-section-label">Transcript</div>
        <div class="ev-chat">
          @if (c.transcriptJson?.length) {
            @for (m of c.transcriptJson; track $index) {
              <div class="ev-bubble-row" [class.is-user]="m.speaker === 'user'">
                <div class="ev-bubble" [class.ev-bubble--ai]="m.speaker === 'assistant'" [class.ev-bubble--user]="m.speaker === 'user'">
                  <div class="ev-bubble__who">{{ m.speaker === 'assistant' ? '🤖 AI' : '👤 Parent' }}</div>
                  {{ m.text }}
                </div>
              </div>
            }
          } @else if (c.transcript) {
            <p class="text-secondary-ev small">{{ c.transcript }}</p>
          } @else {
            <p class="text-muted-ev small text-center py-3">No transcript available for this call.</p>
          }
        </div>
      }
    </ev-drawer>
  `,
  styles: [`
    .ev-call-info { background: var(--ev-surface-2); border-radius: 16px; padding: 1.2rem; }
    .ev-call-meta { display: grid; grid-template-columns: 1fr 1fr; gap: .8rem; margin-top: 1rem; padding-top: 1rem; border-top: 1px solid var(--ev-border); }
    .ev-call-meta span { display: block; font-size: .74rem; color: var(--ev-text-muted); text-transform: uppercase; letter-spacing: .04em; }
    .ev-call-meta b { font-size: .9rem; }
    .ev-outcomes { display: flex; gap: .5rem; flex-wrap: wrap; margin: 1.2rem 0; }
    .ev-outcome { flex: 1; min-width: 120px; display: flex; align-items: center; gap: .45rem; font-size: .82rem; font-weight: 600; color: var(--ev-text-muted); background: var(--ev-surface-2); border: 1px solid var(--ev-border); border-radius: 12px; padding: .6rem .8rem; }
    .ev-outcome.is-on { color: #047857; background: var(--ev-success-soft); border-color: transparent; }
    .ev-outcome.is-warn { color: #B91C1C; background: var(--ev-danger-soft); border-color: transparent; }
    .ev-section-label { font-size: .78rem; text-transform: uppercase; letter-spacing: .05em; color: var(--ev-text-secondary); font-weight: 700; margin: 1.3rem 0 .6rem; }
    .ev-audio audio { border-radius: 12px; }
    .ev-summary { background: linear-gradient(135deg, var(--ev-primary-soft), #fff); border: 1px solid var(--ev-primary-light); border-radius: 14px; padding: .9rem 1rem; font-size: .9rem; color: var(--ev-text); }
    .ev-summary i { color: var(--ev-primary); }
    .ev-chat { display: flex; flex-direction: column; gap: .6rem; background: var(--ev-surface-2); border-radius: 16px; padding: 1.1rem; }
    .ev-bubble-row { display: flex; }
    .ev-bubble-row.is-user { justify-content: flex-end; }
    .ev-bubble { max-width: 80%; padding: .6rem .85rem; border-radius: 16px; font-size: .88rem; line-height: 1.45; box-shadow: var(--ev-shadow-xs); }
    .ev-bubble__who { font-size: .68rem; font-weight: 700; opacity: .7; margin-bottom: .15rem; }
    .ev-bubble--ai { background: #fff; border: 1px solid var(--ev-border); border-bottom-left-radius: 4px; }
    .ev-bubble--user { background: var(--ev-grad-primary); color: #fff; border-bottom-right-radius: 4px; }
    .ev-bubble--user .ev-bubble__who { opacity: .85; }
  `],
})
export class CallDetailPanelComponent {
  readonly call = input<Call | null>(null);
  readonly close = output<void>();
}
