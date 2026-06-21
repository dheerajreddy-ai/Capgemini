import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import {
  StaffOperationsService,
  LogAbsenceRequest,
  CreatePtmRequest,
} from '../../core/services/staff-operations.service';
import { StaffAbsence, PtmSchedule } from '../../core/models/models';
import { ToastService } from '../../core/services/toast.service';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'ev-staff-operations',
  standalone: true,
  imports: [FormsModule, DatePipe, SkeletonComponent, EmptyStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Staff Operations</h1>
        <p class="ev-page-sub">Log teacher absences with substitute alerts and schedule Parent-Teacher Meetings</p>
      </div>
    </div>

    <!-- Tabs -->
    <ul class="nav nav-tabs mb-4">
      <li class="nav-item">
        <button class="nav-link" [class.active]="tab() === 'absences'" (click)="tab.set('absences')">
          <i class="bi bi-person-x me-1"></i>Teacher Absences
        </button>
      </li>
      <li class="nav-item">
        <button class="nav-link" [class.active]="tab() === 'ptm'" (click)="tab.set('ptm')">
          <i class="bi bi-people me-1"></i>PTM Scheduler
        </button>
      </li>
    </ul>

    <!-- ===== ABSENCES TAB ===== -->
    @if (tab() === 'absences') {
      <div class="row g-4">
        <!-- Log Absence Form -->
        <div class="col-lg-5">
          <div class="ev-card p-4">
            <div class="fw-semibold mb-3"><i class="bi bi-plus-circle me-2 text-primary"></i>Log Absence</div>
            <form (ngSubmit)="logAbsence()" #absenceForm="ngForm">
              <div class="mb-3">
                <label class="form-label fw-semibold">Teacher Name <span class="text-danger">*</span></label>
                <input class="form-control" [(ngModel)]="absenceReq.teacherName" name="teacherName" required />
              </div>
              <div class="mb-3">
                <label class="form-label fw-semibold">Teacher Phone</label>
                <input class="form-control" [(ngModel)]="absenceReq.teacherPhone" name="teacherPhone"
                  placeholder="+91..." />
              </div>
              <div class="row g-2 mb-3">
                <div class="col">
                  <label class="form-label fw-semibold">Affected Class</label>
                  <input class="form-control" [(ngModel)]="absenceReq.affectedClass" name="affClass"
                    placeholder="e.g. 8" />
                </div>
                <div class="col-4">
                  <label class="form-label fw-semibold">Section</label>
                  <input class="form-control" [(ngModel)]="absenceReq.affectedSection" name="affSection"
                    placeholder="A" />
                </div>
              </div>
              <div class="mb-3">
                <label class="form-label fw-semibold">Absence Date <span class="text-danger">*</span></label>
                <input type="date" class="form-control" [(ngModel)]="absenceReq.absenceDate" name="absDate" required />
              </div>
              <hr class="my-3" />
              <div class="mb-2 text-muted small fw-semibold">Substitute Teacher (optional)</div>
              <div class="mb-3">
                <label class="form-label small">Name</label>
                <input class="form-control" [(ngModel)]="absenceReq.substituteTeacherName" name="subName"
                  placeholder="Substitute name" />
              </div>
              <div class="mb-3">
                <label class="form-label small">Phone (WhatsApp alert will be sent)</label>
                <input class="form-control" [(ngModel)]="absenceReq.substituteTeacherPhone" name="subPhone"
                  placeholder="+91..." />
              </div>
              <div class="mb-3">
                <label class="form-label fw-semibold">Notes</label>
                <textarea class="form-control" rows="2" [(ngModel)]="absenceReq.notes" name="notes"></textarea>
              </div>
              <div class="form-check mb-4">
                <input class="form-check-input" type="checkbox" id="notifyParents"
                  [(ngModel)]="absenceReq.notifyParents" name="notifyParents" />
                <label class="form-check-label" for="notifyParents">
                  Notify parents of affected class via WhatsApp
                </label>
              </div>
              <button type="submit" class="btn btn-primary w-100" [disabled]="saving() || !absenceForm.valid">
                @if (saving() && activeAction() === 'absence') {
                  <span class="spinner-border spinner-border-sm me-2"></span>
                } @else {
                  <i class="bi bi-send me-1"></i>
                }
                Log Absence & Send Alerts
              </button>
            </form>
          </div>
        </div>

        <!-- Absences List -->
        <div class="col-lg-7">
          <div class="ev-card">
            <div class="ev-card-head">Recent Absences (Last 30 Days)</div>
            @if (loadingAbsences()) {
              <div class="ev-card-pad"><ev-skeleton [rows]="4" [height]="16" /></div>
            } @else if (absences().length === 0) {
              <ev-empty-state icon="bi-person-check" title="No absences logged"
                message="Use the form to log a teacher absence." />
            } @else {
              <div class="table-responsive">
                <table class="table table-hover mb-0 align-middle">
                  <thead class="table-light">
                    <tr>
                      <th>Teacher</th>
                      <th>Date</th>
                      <th>Class</th>
                      <th>Substitute</th>
                      <th>Alerts</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (a of absences(); track a.id) {
                      <tr>
                        <td>
                          <div class="fw-semibold small">{{ a.teacherName }}</div>
                          @if (a.teacherPhone) {
                            <div class="text-muted" style="font-size:.7rem">{{ a.teacherPhone }}</div>
                          }
                        </td>
                        <td class="small">{{ a.absenceDate | date:'dd MMM yyyy' }}</td>
                        <td class="small">
                          {{ a.affectedClass ?? '—' }}{{ a.affectedSection ? ' ' + a.affectedSection : '' }}
                        </td>
                        <td class="small">
                          {{ a.substituteTeacherName ?? '—' }}
                        </td>
                        <td>
                          <div class="d-flex gap-1 flex-wrap">
                            <span class="badge" [class.bg-success]="a.substituteAlertSent"
                              [class.bg-secondary]="!a.substituteAlertSent">
                              <i class="bi bi-person me-1"></i>Sub
                            </span>
                            <span class="badge" [class.bg-success]="a.parentNotificationSent"
                              [class.bg-secondary]="!a.parentNotificationSent">
                              <i class="bi bi-people me-1"></i>Parents
                            </span>
                          </div>
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>
      </div>
    }

    <!-- ===== PTM TAB ===== -->
    @if (tab() === 'ptm') {
      <div class="row g-4">
        <!-- Schedule PTM Form -->
        <div class="col-lg-5">
          <div class="ev-card p-4">
            <div class="fw-semibold mb-3"><i class="bi bi-plus-circle me-2 text-primary"></i>Schedule PTM</div>
            <form (ngSubmit)="schedulePtm()" #ptmForm="ngForm">
              <div class="mb-3">
                <label class="form-label fw-semibold">Title <span class="text-danger">*</span></label>
                <input class="form-control" [(ngModel)]="ptmReq.title" name="title" required
                  placeholder="e.g. Term 1 Parent-Teacher Meeting" />
              </div>
              <div class="mb-3">
                <label class="form-label fw-semibold">PTM Date <span class="text-danger">*</span></label>
                <input type="date" class="form-control" [(ngModel)]="ptmReq.ptmDate" name="ptmDate"
                  [min]="today" required />
              </div>
              <div class="row g-2 mb-3">
                <div class="col">
                  <label class="form-label fw-semibold">Class <span class="text-muted small">(blank = all)</span></label>
                  <input class="form-control" [(ngModel)]="ptmReq.class" name="ptmClass" placeholder="e.g. 8" />
                </div>
                <div class="col-4">
                  <label class="form-label fw-semibold">Section</label>
                  <input class="form-control" [(ngModel)]="ptmReq.section" name="ptmSection" placeholder="A" />
                </div>
              </div>
              <div class="mb-4">
                <label class="form-label fw-semibold">Notes</label>
                <textarea class="form-control" rows="2" [(ngModel)]="ptmReq.notes" name="ptmNotes"
                  placeholder="Venue, time, agenda..."></textarea>
              </div>
              <div class="alert alert-info py-2 mb-4 small">
                <i class="bi bi-bell me-1"></i>
                WhatsApp reminders will be sent automatically <strong>3 days</strong> and <strong>1 day</strong> before the PTM.
              </div>
              <button type="submit" class="btn btn-primary w-100" [disabled]="saving() || !ptmForm.valid">
                @if (saving() && activeAction() === 'ptm') {
                  <span class="spinner-border spinner-border-sm me-2"></span>
                } @else {
                  <i class="bi bi-calendar-plus me-1"></i>
                }
                Schedule PTM
              </button>
            </form>
          </div>
        </div>

        <!-- PTM List -->
        <div class="col-lg-7">
          <div class="ev-card">
            <div class="ev-card-head">Upcoming PTMs</div>
            @if (loadingPtm()) {
              <div class="ev-card-pad"><ev-skeleton [rows]="3" [height]="16" /></div>
            } @else if (ptmSchedules().length === 0) {
              <ev-empty-state icon="bi-calendar-x" title="No PTMs scheduled"
                message="Use the form to schedule a parent-teacher meeting." />
            } @else {
              <div class="table-responsive">
                <table class="table table-hover mb-0 align-middle">
                  <thead class="table-light">
                    <tr>
                      <th>Title</th>
                      <th>Date</th>
                      <th>Class</th>
                      <th>Reminders</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (p of ptmSchedules(); track p.id) {
                      <tr>
                        <td class="fw-semibold small">{{ p.title }}</td>
                        <td class="small">
                          <span [class.text-danger]="isPast(p.ptmDate)"
                                [class.text-warning]="isSoon(p.ptmDate)">
                            {{ p.ptmDate | date:'dd MMM yyyy' }}
                          </span>
                          @if (isSoon(p.ptmDate)) {
                            <span class="badge bg-warning text-dark ms-1">Soon</span>
                          }
                        </td>
                        <td class="small text-muted">
                          {{ p.class ? p.class + (p.section ? ' ' + p.section : '') : 'All classes' }}
                        </td>
                        <td>
                          <div class="d-flex gap-1">
                            <span class="badge" [class.bg-success]="p.reminder3DaySent"
                              [class.bg-secondary]="!p.reminder3DaySent" title="3-day reminder">
                              3d
                            </span>
                            <span class="badge" [class.bg-success]="p.reminder1DaySent"
                              [class.bg-secondary]="!p.reminder1DaySent" title="1-day reminder">
                              1d
                            </span>
                          </div>
                        </td>
                        <td>
                          <button class="btn btn-ghost btn-icon btn-sm text-danger"
                            [disabled]="saving()" (click)="cancelPtm(p)"
                            title="Cancel PTM">
                            <i class="bi bi-x-circle"></i>
                          </button>
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>
      </div>
    }
  `,
})
export class StaffOperationsComponent implements OnInit {
  private readonly service = inject(StaffOperationsService);
  private readonly toast = inject(ToastService);

  readonly tab = signal<'absences' | 'ptm'>('absences');
  readonly saving = signal(false);
  readonly activeAction = signal<'absence' | 'ptm' | null>(null);
  readonly loadingAbsences = signal(true);
  readonly loadingPtm = signal(true);
  readonly absences = signal<StaffAbsence[]>([]);
  readonly ptmSchedules = signal<PtmSchedule[]>([]);

  readonly today = new Date().toISOString().split('T')[0];

  absenceReq: LogAbsenceRequest = {
    teacherName: '',
    absenceDate: this.today,
    notifyParents: true,
  };

  ptmReq: CreatePtmRequest = {
    title: '',
    ptmDate: '',
  };

  ngOnInit(): void {
    this.loadAbsences();
    this.loadPtm();
  }

  loadAbsences(): void {
    this.loadingAbsences.set(true);
    this.service.getAbsences().subscribe({
      next: (a) => { this.absences.set(a); this.loadingAbsences.set(false); },
      error: () => this.loadingAbsences.set(false),
    });
  }

  loadPtm(): void {
    this.loadingPtm.set(true);
    this.service.getPtmSchedules().subscribe({
      next: (p) => { this.ptmSchedules.set(p); this.loadingPtm.set(false); },
      error: () => this.loadingPtm.set(false),
    });
  }

  logAbsence(): void {
    this.saving.set(true);
    this.activeAction.set('absence');
    this.service.logAbsence(this.absenceReq).subscribe({
      next: () => {
        this.saving.set(false);
        this.activeAction.set(null);
        this.toast.success('Absence logged and alerts sent');
        this.absenceReq = { teacherName: '', absenceDate: this.today, notifyParents: true };
        this.loadAbsences();
      },
      error: (err) => {
        this.toast.error(err?.error?.message ?? 'Failed to log absence');
        this.saving.set(false);
        this.activeAction.set(null);
      },
    });
  }

  schedulePtm(): void {
    this.saving.set(true);
    this.activeAction.set('ptm');
    this.service.schedulePtm(this.ptmReq).subscribe({
      next: () => {
        this.saving.set(false);
        this.activeAction.set(null);
        this.toast.success('PTM scheduled — reminders will be sent automatically');
        this.ptmReq = { title: '', ptmDate: '' };
        this.loadPtm();
      },
      error: (err) => {
        this.toast.error(err?.error?.message ?? 'Failed to schedule PTM');
        this.saving.set(false);
        this.activeAction.set(null);
      },
    });
  }

  cancelPtm(p: PtmSchedule): void {
    if (!confirm(`Cancel "${p.title}"? This will stop any pending reminders.`)) return;
    this.saving.set(true);
    this.service.deletePtm(p.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('PTM cancelled');
        this.loadPtm();
      },
      error: () => {
        this.saving.set(false);
        this.toast.error('Failed to cancel PTM');
      },
    });
  }

  isPast(dateStr: string): boolean {
    return new Date(dateStr) < new Date();
  }

  isSoon(dateStr: string): boolean {
    const diff = (new Date(dateStr).getTime() - Date.now()) / (1000 * 60 * 60 * 24);
    return diff >= 0 && diff <= 3;
  }
}
