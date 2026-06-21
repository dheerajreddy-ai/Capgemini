import { Component, ChangeDetectionStrategy, inject, input, output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CampaignsService, CreateCampaignRequest } from '../../core/services/campaigns.service';
import { ToastService } from '../../core/services/toast.service';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';

@Component({
  selector: 'ev-campaign-wizard',
  standalone: true,
  imports: [FormsModule, DrawerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ev-drawer [open]="open()" size="lg" title="New Campaign" subtitle="Set up an automated parent call campaign"
      (close)="reset(); close.emit()">
      <!-- Stepper -->
      <div class="ev-stepper">
        @for (s of steps; track s.n) {
          <div class="ev-step" [class.is-active]="step() === s.n" [class.is-done]="step() > s.n">
            <div class="ev-step__dot">@if (step() > s.n) { <i class="bi bi-check"></i> } @else { {{ s.n }} }</div>
            <span>{{ s.label }}</span>
          </div>
        }
      </div>

      <!-- Step 1: type -->
      @if (step() === 1) {
        <div class="row g-3">
          @for (t of types; track t.value) {
            <div class="col-md-6">
              <button class="ev-type-card w-100" [class.is-sel]="model.campaignType === t.value" (click)="model.campaignType = t.value">
                <div class="ev-type-ico" [class]="t.cls"><i class="bi {{ t.icon }}"></i></div>
                <h6 class="mb-1">{{ t.title }}</h6>
                <p class="small text-secondary-ev mb-0">{{ t.desc }}</p>
              </button>
            </div>
          }
        </div>
        <div class="mt-3"><label class="form-label">Campaign name</label><input class="form-control" [(ngModel)]="model.campaignName" placeholder="e.g. June Fee Reminders" /></div>
      }

      <!-- Step 2: audience -->
      @if (step() === 2) {
        <label class="form-label">Who should we call?</label>
        <div class="d-flex flex-column gap-2">
          <label class="ev-radio"><input type="radio" name="aud" [checked]="audience() === 'all'" (change)="setAudience('all')" /><span>All students</span></label>
          @if (model.campaignType === 'FeeReminder') {
            <label class="ev-radio"><input type="radio" name="aud" [checked]="audience() === 'overdue'" (change)="setAudience('overdue')" /><span>Only overdue / unpaid fees</span></label>
          } @else {
            <label class="ev-radio"><input type="radio" name="aud" [checked]="audience() === 'weak'" (change)="setAudience('weak')" /><span>Only students below 60% marks</span></label>
          }
          <label class="ev-radio"><input type="radio" name="aud" [checked]="audience() === 'classes'" (change)="setAudience('classes')" /><span>Specific classes</span></label>
        </div>
        @if (audience() === 'classes') {
          <input class="form-control mt-2" placeholder="Comma separated, e.g. 10th A, 9th B" [(ngModel)]="classesRaw" />
        }
        <div class="ev-preview-count mt-3">
          <i class="bi bi-people-fill me-2"></i><b>{{ previewCount() ?? '—' }}</b> parents will be called
        </div>
      }

      <!-- Step 3: schedule -->
      @if (step() === 3) {
        <label class="form-label">When should it run?</label>
        <div class="d-flex flex-column gap-2">
          <label class="ev-radio"><input type="radio" name="sch" [checked]="!scheduled()" (change)="scheduled.set(false)" /><span><b>Call now</b> — start immediately</span></label>
          <label class="ev-radio"><input type="radio" name="sch" [checked]="scheduled()" (change)="scheduled.set(true)" /><span><b>Schedule for later</b></span></label>
        </div>
        @if (scheduled()) { <input type="datetime-local" class="form-control mt-2" [(ngModel)]="scheduleAt" /> }
      }

      <!-- Step 4: confirm -->
      @if (step() === 4) {
        <div class="ev-confirm-box">
          <div class="ev-info-row"><span>Campaign</span><b>{{ model.campaignName || 'Untitled' }}</b></div>
          <div class="ev-info-row"><span>Type</span><b>{{ model.campaignType }}</b></div>
          <div class="ev-info-row"><span>Audience</span><b>{{ audienceLabel() }}</b></div>
          <div class="ev-info-row"><span>Parents</span><b>{{ previewCount() ?? '—' }}</b></div>
          <div class="ev-info-row"><span>Schedule</span><b>{{ scheduled() ? scheduleAt : 'Immediately' }}</b></div>
        </div>
        <div class="ev-hint mt-3"><i class="bi bi-info-circle me-2"></i>Calls are placed ~2s apart to respect carrier limits.</div>
      }

      <div slot="footer">
        @if (step() > 1) { <button class="btn btn-soft" (click)="step.set(step() - 1)">Back</button> }
        @if (step() < 4) {
          <button class="btn btn-primary" [disabled]="!canNext()" (click)="next()">Continue</button>
        } @else {
          <button class="btn btn-primary" [disabled]="submitting()" (click)="submit()">
            @if (submitting()) { <span class="spinner-border spinner-border-sm me-2"></span> }
            Launch campaign
          </button>
        }
      </div>
    </ev-drawer>
  `,
  styleUrl: './campaign-wizard.component.scss',
})
export class CampaignWizardComponent {
  private readonly service = inject(CampaignsService);
  private readonly toast = inject(ToastService);

  readonly open = input(false);
  readonly close = output<void>();
  readonly created = output<void>();

  readonly step = signal(1);
  readonly audience = signal<'all' | 'overdue' | 'weak' | 'classes'>('all');
  readonly scheduled = signal(false);
  readonly previewCount = signal<number | null>(null);
  readonly submitting = signal(false);

  classesRaw = '';
  scheduleAt = '';
  model: { campaignName: string; campaignType: 'FeeReminder' | 'ProgressUpdate' } = {
    campaignName: '', campaignType: 'FeeReminder',
  };

  readonly steps = [
    { n: 1, label: 'Type' }, { n: 2, label: 'Audience' }, { n: 3, label: 'Schedule' }, { n: 4, label: 'Confirm' },
  ];
  readonly types = [
    { value: 'FeeReminder' as const, title: 'Fee Reminder', desc: 'Remind parents about pending fees and collect confirmations.', icon: 'bi-cash-coin', cls: 'ev-type-ico--fee' },
    { value: 'ProgressUpdate' as const, title: 'Progress Update', desc: 'Share marks, attendance and weak subjects with parents.', icon: 'bi-graph-up-arrow', cls: 'ev-type-ico--progress' },
  ];

  readonly canNext = computed(() => {
    if (this.step() === 1) return !!this.model.campaignName.trim();
    return true;
  });

  audienceLabel(): string {
    return { all: 'All students', overdue: 'Overdue / unpaid fees', weak: 'Below 60% marks', classes: this.classesRaw || 'Selected classes' }[this.audience()];
  }

  setAudience(a: 'all' | 'overdue' | 'weak' | 'classes'): void {
    this.audience.set(a);
    this.fetchPreview();
  }

  next(): void {
    if (this.step() === 1) this.fetchPreview();
    this.step.update((s) => Math.min(4, s + 1));
  }

  private buildRequest(): CreateCampaignRequest {
    return {
      campaignName: this.model.campaignName,
      campaignType: this.model.campaignType,
      filters: {
        classes: this.audience() === 'classes' ? this.classesRaw.split(',').map((c) => c.trim()).filter(Boolean) : [],
        feesStatus: this.audience() === 'overdue' ? 'Overdue' : 'All',
        belowMarksThreshold: this.audience() === 'weak' ? 60 : undefined,
        specificStudentIds: [],
      },
      scheduledAt: this.scheduled() && this.scheduleAt ? new Date(this.scheduleAt).toISOString() : null,
    };
  }

  private fetchPreview(): void {
    this.service.preview(this.buildRequest()).subscribe({
      next: (r) => this.previewCount.set(r.count),
      error: () => this.previewCount.set(null),
    });
  }

  submit(): void {
    this.submitting.set(true);
    this.service.create(this.buildRequest()).subscribe({
      next: () => {
        this.submitting.set(false);
        this.toast.success('Campaign launched', this.scheduled() ? 'Scheduled successfully.' : 'Calls are starting now.');
        this.reset();
        this.created.emit();
      },
      error: () => this.submitting.set(false),
    });
  }

  reset(): void {
    this.step.set(1); this.audience.set('all'); this.scheduled.set(false);
    this.previewCount.set(null); this.classesRaw = ''; this.scheduleAt = '';
    this.model = { campaignName: '', campaignType: 'FeeReminder' };
  }
}
