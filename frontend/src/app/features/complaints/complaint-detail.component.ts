import { Component, ChangeDetectionStrategy, inject, input, output, effect, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { ComplaintsService } from '../../core/services/complaints.service';
import { Complaint } from '../../core/models/models';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';

@Component({
  selector: 'ev-complaint-detail',
  standalone: true,
  imports: [FormsModule, DatePipe, DrawerComponent, InitialsPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ev-drawer [open]="!!complaint()" size="lg" title="Complaint Details" (close)="close.emit()">
      @if (complaint(); as c) {
        <div class="d-flex align-items-center gap-3 mb-3">
          <div class="ev-avatar ev-avatar--lg">{{ c.parentName | evInitials }}</div>
          <div>
            <div class="fw-bold fs-5">{{ c.parentName }}</div>
            <div class="text-secondary-ev small">{{ c.studentName }} · {{ c.studentClass }} · {{ c.parentPhone }}</div>
            <div class="text-muted-ev small">{{ c.createdAt | date:'medium' }}</div>
          </div>
        </div>

        <div class="ev-section-label">What the parent said</div>
        <div class="ev-verbatim">{{ c.complaintText }}</div>

        @if (c.complaintSummary) {
          <div class="ev-section-label">AI Summary</div>
          <div class="ev-summary"><i class="bi bi-stars me-2"></i>{{ c.complaintSummary }}</div>
        }

        @if (c.recordingUrl) {
          <div class="ev-section-label">Recording</div>
          <audio controls [src]="c.recordingUrl" class="w-100"></audio>
        }

        <div class="ev-section-label">Manage</div>
        <div class="row g-3">
          <div class="col-md-6">
            <label class="form-label">Category</label>
            <select class="form-select" [(ngModel)]="form.category">
              <option>Teacher</option><option>Fees</option><option>Facility</option><option>Academic</option><option>Behaviour</option><option>Other</option>
            </select>
          </div>
          <div class="col-md-6">
            <label class="form-label">Priority</label>
            <select class="form-select" [(ngModel)]="form.priority">
              <option>Low</option><option>Medium</option><option>High</option><option>Urgent</option>
            </select>
          </div>
          <div class="col-12">
            <label class="form-label">Teacher notes</label>
            <textarea class="form-control" rows="3" [(ngModel)]="form.teacherNotes" placeholder="Add internal notes…"></textarea>
          </div>
        </div>

        <div class="ev-section-label">Status</div>
        <div class="d-flex gap-2 flex-wrap">
          @for (s of statuses; track s) {
            <button class="ev-status-btn" [class.is-active]="form.status === s" (click)="form.status = s">{{ label(s) }}</button>
          }
        </div>
      }

      <div slot="footer">
        <button class="btn btn-soft" (click)="close.emit()">Cancel</button>
        <button class="btn btn-primary" [disabled]="saving()" (click)="save()">
          @if (saving()) { <span class="spinner-border spinner-border-sm me-2"></span> } Save
        </button>
      </div>
    </ev-drawer>
  `,
  styles: [`
    .ev-section-label { font-size: .78rem; text-transform: uppercase; letter-spacing: .05em; color: var(--ev-text-secondary); font-weight: 700; margin: 1.3rem 0 .6rem; }
    .ev-verbatim { background: var(--ev-surface-2); border-left: 3px solid var(--ev-border-strong); border-radius: 0 12px 12px 0; padding: .9rem 1.1rem; font-style: italic; color: var(--ev-text); line-height: 1.6; }
    .ev-summary { background: linear-gradient(135deg, var(--ev-primary-soft), #fff); border: 1px solid var(--ev-primary-light); border-radius: 14px; padding: .9rem 1rem; font-size: .9rem; }
    .ev-summary i { color: var(--ev-primary); }
    .ev-status-btn { background: #fff; border: 1.5px solid var(--ev-border-strong); border-radius: 999px; padding: .45rem 1rem; font-weight: 600; font-size: .85rem; color: var(--ev-text-secondary); transition: all .15s ease; }
    .ev-status-btn:hover { border-color: var(--ev-primary); color: var(--ev-primary); }
    .ev-status-btn.is-active { background: var(--ev-grad-primary); color: #fff; border-color: transparent; box-shadow: var(--ev-shadow-primary); }
  `],
})
export class ComplaintDetailComponent {
  private readonly service = inject(ComplaintsService);

  readonly complaint = input<Complaint | null>(null);
  readonly close = output<void>();
  readonly updated = output<void>();

  readonly saving = signal(false);
  readonly statuses = ['New', 'Read', 'InProgress', 'Resolved', 'Closed'];

  form: { category: string; priority: string; status: string; teacherNotes: string } = {
    category: 'Other', priority: 'Medium', status: 'New', teacherNotes: '',
  };

  constructor() {
    effect(() => {
      const c = this.complaint();
      if (c) this.form = { category: c.category, priority: c.priority, status: c.status, teacherNotes: c.teacherNotes ?? '' };
    });
  }

  label(s: string): string { return s.replace(/([A-Z])/g, ' $1').trim(); }

  save(): void {
    const c = this.complaint();
    if (!c) return;
    this.saving.set(true);
    this.service.update(c.id, this.form as Partial<Complaint>).subscribe({
      next: () => { this.saving.set(false); this.updated.emit(); },
      error: () => this.saving.set(false),
    });
  }
}
