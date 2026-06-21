import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { ParentEngagementService } from '../../core/services/parent-engagement.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'ev-parent-engagement',
  standalone: true,
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Parent Engagement</h1>
        <p class="ev-page-sub">Automated WhatsApp messages that keep parents informed and involved</p>
      </div>
    </div>

    <div class="row g-4 mb-5">
      <!-- Birthday Wishes -->
      <div class="col-md-4">
        <div class="ev-card h-100 p-4">
          <div class="d-flex align-items-center gap-3 mb-3">
            <div class="rounded-circle d-flex align-items-center justify-content-center"
              style="width:48px;height:48px;font-size:1.4rem;background:#e83e8c1a;color:#e83e8c;flex-shrink:0">
              <i class="bi bi-cake2"></i>
            </div>
            <div>
              <div class="fw-bold">Birthday Wishes</div>
              <div class="text-muted" style="font-size:.75rem">Daily at 9AM IST</div>
            </div>
          </div>
          <p class="text-muted small mb-4">
            Personalised WhatsApp greetings to parents of students whose birthday falls today.
            Runs automatically every morning — use the button below to trigger manually.
          </p>
          <button class="btn btn-sm w-100" style="background:#e83e8c;color:#fff"
            [disabled]="busy()"
            (click)="sendBirthday()">
            @if (busy() && activeAction() === 'birthday') {
              <span class="spinner-border spinner-border-sm me-2"></span>
            } @else {
              <i class="bi bi-send me-1"></i>
            }
            Send Today's Wishes
          </button>
        </div>
      </div>

      <!-- Achievement Alerts -->
      <div class="col-md-4">
        <div class="ev-card h-100 p-4">
          <div class="d-flex align-items-center gap-3 mb-3">
            <div class="rounded-circle d-flex align-items-center justify-content-center"
              style="width:48px;height:48px;font-size:1.4rem;background:#fd7e141a;color:#fd7e14;flex-shrink:0">
              <i class="bi bi-star-fill"></i>
            </div>
            <div>
              <div class="fw-bold">Achievement Alerts</div>
              <div class="text-muted" style="font-size:.75rem">Auto-triggered on marks update</div>
            </div>
          </div>
          <p class="text-muted small mb-4">
            When a student's percentage crosses the school's <em>Achievement Threshold</em> (default 80%)
            after a marks update, the system automatically congratulates parents via WhatsApp.
            30-day cooldown per student avoids repetition.
          </p>
          <div class="alert alert-warning py-2 mb-0 small">
            <i class="bi bi-lightning-charge me-1"></i>
            Fires automatically — no manual trigger needed.
          </div>
        </div>
      </div>

      <!-- Weekly Summary -->
      <div class="col-md-4">
        <div class="ev-card h-100 p-4">
          <div class="d-flex align-items-center gap-3 mb-3">
            <div class="rounded-circle d-flex align-items-center justify-content-center"
              style="width:48px;height:48px;font-size:1.4rem;background:#1987541a;color:#198754;flex-shrink:0">
              <i class="bi bi-calendar-week"></i>
            </div>
            <div>
              <div class="fw-bold">Weekly Child Summary</div>
              <div class="text-muted" style="font-size:.75rem">Monday at 7PM IST</div>
            </div>
          </div>
          <p class="text-muted small mb-4">
            Every parent receives a WhatsApp with their child's attendance %, academic score,
            and fees status. 6-day cooldown prevents duplicate sends.
          </p>
          <button class="btn btn-sm w-100" style="background:#198754;color:#fff"
            [disabled]="busy()"
            (click)="sendWeekly()">
            @if (busy() && activeAction() === 'weekly') {
              <span class="spinner-border spinner-border-sm me-2"></span>
            } @else {
              <i class="bi bi-send me-1"></i>
            }
            Send Summaries Now
          </button>
        </div>
      </div>
    </div>

    <!-- What the messages look like -->
    <div class="ev-card p-4">
      <div class="fw-semibold mb-3"><i class="bi bi-chat-text me-2 text-primary"></i>Example WhatsApp Messages</div>
      <div class="row g-3">
        <div class="col-md-4">
          <div class="p-3 rounded-3 bg-light border" style="font-size:.8rem;white-space:pre-line">🎂 <strong>Happy Birthday — ABC School</strong>

Dear Ravi Kumar,

Wishing <strong>Priya Ravi</strong> a very Happy Birthday! 🎉🌟

May this special day bring joy and success in studies.

Warm regards,
ABC School Family</div>
        </div>
        <div class="col-md-4">
          <div class="p-3 rounded-3 bg-light border" style="font-size:.8rem;white-space:pre-line">🌟 <strong>Achievement Alert — ABC School</strong>

Congratulations, Ravi Kumar!

Your child <strong>Priya Ravi</strong> (Class 8) achieved an excellent score of <strong>92%</strong> in their recent exam! 🏆

Keep up the great work — we're proud!

— EduVoice, ABC School</div>
        </div>
        <div class="col-md-4">
          <div class="p-3 rounded-3 bg-light border" style="font-size:.8rem;white-space:pre-line">📊 <strong>Weekly Update — ABC School</strong>

Hello Ravi Kumar,

<strong>Priya Ravi</strong>'s (Class 8) weekly update:

📅 Attendance: 88% (22/25 days)
📚 Academic Score: 92%
💰 Fees: ✅ Fully paid

For queries, contact the school.

— EduVoice, ABC School</div>
        </div>
      </div>
    </div>
  `,
})
export class ParentEngagementComponent {
  private readonly service = inject(ParentEngagementService);
  private readonly toast = inject(ToastService);

  readonly busy = signal(false);
  readonly activeAction = signal<'birthday' | 'weekly' | null>(null);

  sendBirthday(): void {
    this.run('birthday', () => this.service.sendBirthdayWishes(), 'birthday wishes');
  }

  sendWeekly(): void {
    this.run('weekly', () => this.service.sendWeeklySummaries(), 'weekly summaries');
  }

  private run(
    action: 'birthday' | 'weekly',
    call: () => ReturnType<ParentEngagementService['sendBirthdayWishes']>,
    label: string,
  ): void {
    this.busy.set(true);
    this.activeAction.set(action);
    call().subscribe({
      next: (count) => {
        this.busy.set(false);
        this.activeAction.set(null);
        this.toast.success(`Sent ${count} ${label}`);
      },
      error: () => {
        this.busy.set(false);
        this.activeAction.set(null);
        this.toast.error('Failed to send messages');
      },
    });
  }
}
