import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ExamScheduleService, CreateExamScheduleRequest } from '../../core/services/exam-schedule.service';
import { ExamSchedule, ExamType } from '../../core/models/models';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'ev-exam-schedule',
  standalone: true,
  imports: [FormsModule, RelativeTimePipe, SkeletonComponent, EmptyStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Exam Schedule</h1>
        <p class="ev-page-sub">Auto-call parents 3 days & 1 day before each paper</p>
      </div>
      <button class="btn btn-primary" (click)="showForm.set(true)">
        <i class="bi bi-calendar-plus me-2"></i>Add Exam
      </button>
    </div>

    <!-- Add Exam Modal -->
    @if (showForm()) {
      <div class="modal show d-block" style="background:rgba(0,0,0,.5)">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="bi bi-calendar-event me-2 text-primary"></i>Add Exam</h5>
              <button class="btn-close" (click)="cancelForm()"></button>
            </div>
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label fw-semibold">Subject <span class="text-danger">*</span></label>
                <input class="form-control" [(ngModel)]="form.subjectName" placeholder="e.g. Mathematics, Physics" />
              </div>
              <div class="row g-3 mb-3">
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Exam Type</label>
                  <select class="form-select" [(ngModel)]="form.examType">
                    <option value="UnitTest">Unit Test</option>
                    <option value="Midterm">Midterm</option>
                    <option value="Quarterly">Quarterly</option>
                    <option value="HalfYearly">Half Yearly</option>
                    <option value="Final">Final</option>
                    <option value="Annual">Annual</option>
                  </select>
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Exam Date <span class="text-danger">*</span></label>
                  <input type="date" class="form-control" [(ngModel)]="form.examDate" />
                </div>
              </div>
              <div class="row g-3 mb-3">
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Class</label>
                  <input class="form-control" [(ngModel)]="form.class" placeholder="All classes" />
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Section</label>
                  <input class="form-control" [(ngModel)]="form.section" placeholder="All sections" />
                </div>
              </div>
              <div class="mb-3">
                <label class="form-label fw-semibold">Notes</label>
                <input class="form-control" [(ngModel)]="form.notes" placeholder="e.g. Syllabus: Chapters 1-5" />
              </div>
              <div class="alert alert-info py-2 small mb-0">
                <i class="bi bi-info-circle me-2"></i>
                Voice calls will auto-trigger to parents <strong>3 days before</strong> and <strong>1 day before</strong> the exam at 9AM.
              </div>
            </div>
            <div class="modal-footer">
              <button class="btn btn-outline-secondary" (click)="cancelForm()">Cancel</button>
              <button class="btn btn-primary" [disabled]="!isValid() || saving()" (click)="save()">
                @if (saving()) { <span class="spinner-border spinner-border-sm me-2"></span> }
                <i class="bi bi-check-lg me-1"></i>Save
              </button>
            </div>
          </div>
        </div>
      </div>
    }

    <!-- List -->
    @if (loading()) {
      <div class="ev-card ev-card-pad"><ev-skeleton [rows]="5" [height]="20" /></div>
    } @else if (exams().length === 0) {
      <div class="ev-card">
        <ev-empty-state icon="bi-calendar-x" title="No exams scheduled"
          message="Add exam dates to auto-call parents before each paper." />
      </div>
    } @else {
      <div class="ev-card">
        <table class="table table-hover mb-0">
          <thead class="table-light">
            <tr>
              <th>Subject</th>
              <th>Type</th>
              <th>Date</th>
              <th>Target</th>
              <th class="text-center">3-Day Alert</th>
              <th class="text-center">1-Day Alert</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (e of exams(); track e.id) {
              <tr>
                <td class="fw-semibold">{{ e.subjectName }}</td>
                <td><span class="badge bg-light text-dark border">{{ e.examType }}</span></td>
                <td>{{ e.examDate | date:'dd MMM yyyy' }}</td>
                <td class="small text-muted">
                  @if (e.class) { Class {{ e.class }}{{ e.section ? ' ' + e.section : '' }} }
                  @else { All classes }
                </td>
                <td class="text-center">
                  @if (e.reminder3DaySent) {
                    <i class="bi bi-check-circle-fill text-success"></i>
                  } @else {
                    <i class="bi bi-clock text-muted"></i>
                  }
                </td>
                <td class="text-center">
                  @if (e.reminder1DaySent) {
                    <i class="bi bi-check-circle-fill text-success"></i>
                  } @else {
                    <i class="bi bi-clock text-muted"></i>
                  }
                </td>
                <td>
                  <button class="btn btn-sm btn-outline-danger" (click)="remove(e.id)" title="Delete">
                    <i class="bi bi-trash"></i>
                  </button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
})
export class ExamScheduleComponent implements OnInit {
  private readonly service = inject(ExamScheduleService);

  readonly loading = signal(true);
  readonly exams = signal<ExamSchedule[]>([]);
  readonly showForm = signal(false);
  readonly saving = signal(false);

  form: CreateExamScheduleRequest = this.emptyForm();

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.list().subscribe({
      next: (r) => { this.exams.set(r.items ?? []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  isValid(): boolean {
    return !!this.form.subjectName.trim() && !!this.form.examDate;
  }

  save(): void {
    if (!this.isValid()) return;
    this.saving.set(true);
    this.service.create(this.form).subscribe({
      next: () => { this.saving.set(false); this.showForm.set(false); this.form = this.emptyForm(); this.load(); },
      error: () => this.saving.set(false),
    });
  }

  remove(id: string): void {
    this.service.delete(id).subscribe({ next: () => this.load() });
  }

  cancelForm(): void { this.showForm.set(false); this.form = this.emptyForm(); }

  private emptyForm(): CreateExamScheduleRequest {
    return { subjectName: '', examType: 'UnitTest', examDate: '' };
  }
}
