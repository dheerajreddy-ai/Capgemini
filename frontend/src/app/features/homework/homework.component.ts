import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { HomeworkService, CreateHomeworkRequest } from '../../core/services/homework.service';
import { Homework } from '../../core/models/models';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'ev-homework',
  standalone: true,
  imports: [FormsModule, DatePipe, RelativeTimePipe, SkeletonComponent, EmptyStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Homework & Assignments</h1>
        <p class="ev-page-sub">Daily WhatsApp alerts sent to parents at 4PM with today's homework</p>
      </div>
      <button class="btn btn-primary" (click)="showForm.set(true)">
        <i class="bi bi-pencil-square me-2"></i>Add Homework
      </button>
    </div>

    <!-- Filter bar -->
    <div class="ev-card ev-card-pad mb-3 d-flex gap-3 align-items-center">
      <label class="form-label mb-0 fw-semibold">Filter by date</label>
      <input type="date" class="form-control" style="width:200px" [(ngModel)]="filterDate" (change)="load()" />
      @if (filterDate) {
        <button class="btn btn-sm btn-outline-secondary" (click)="filterDate = ''; load()">Clear</button>
      }
    </div>

    <!-- Add Homework Modal -->
    @if (showForm()) {
      <div class="modal show d-block" style="background:rgba(0,0,0,.5)">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="bi bi-pencil-square me-2 text-primary"></i>Add Homework</h5>
              <button class="btn-close" (click)="cancelForm()"></button>
            </div>
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label fw-semibold">Subject <span class="text-danger">*</span></label>
                <input class="form-control" [(ngModel)]="form.subject" placeholder="e.g. Mathematics" />
              </div>
              <div class="mb-3">
                <label class="form-label fw-semibold">Assignment <span class="text-danger">*</span></label>
                <textarea class="form-control" rows="3" [(ngModel)]="form.description"
                  placeholder="Describe the homework / assignment..."></textarea>
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
              <div class="row g-3 mb-3">
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Assigned Date <span class="text-danger">*</span></label>
                  <input type="date" class="form-control" [(ngModel)]="form.assignedDate" />
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-semibold">Due Date</label>
                  <input type="date" class="form-control" [(ngModel)]="form.dueDate" />
                </div>
              </div>
              <div class="alert alert-info py-2 small mb-0">
                <i class="bi bi-info-circle me-2"></i>
                A WhatsApp message will be sent to all parents at <strong>4PM today</strong> with this homework.
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
    } @else if (homeworks().length === 0) {
      <div class="ev-card">
        <ev-empty-state icon="bi-journal-text" title="No homework added"
          message="Add homework to automatically notify parents via WhatsApp at 4PM." />
      </div>
    } @else {
      <div class="ev-card">
        <table class="table table-hover mb-0">
          <thead class="table-light">
            <tr>
              <th>Subject</th>
              <th>Assignment</th>
              <th>Target</th>
              <th>Assigned</th>
              <th>Due</th>
              <th class="text-center">Alert</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (h of homeworks(); track h.id) {
              <tr>
                <td class="fw-semibold">{{ h.subject }}</td>
                <td class="small text-truncate" style="max-width:260px">{{ h.description }}</td>
                <td class="small text-muted">
                  @if (h.class) { Class {{ h.class }}{{ h.section ? ' ' + h.section : '' }} }
                  @else { All classes }
                </td>
                <td class="small">{{ h.assignedDate | date:'dd MMM' }}</td>
                <td class="small">{{ h.dueDate ? (h.dueDate | date:'dd MMM') : '—' }}</td>
                <td class="text-center">
                  @if (h.alertSent) {
                    <span class="badge bg-success">Sent</span>
                  } @else {
                    <span class="badge bg-warning text-dark">Pending</span>
                  }
                </td>
                <td>
                  <button class="btn btn-sm btn-outline-danger" (click)="remove(h.id)" title="Delete">
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
export class HomeworkComponent implements OnInit {
  private readonly service = inject(HomeworkService);

  readonly loading = signal(true);
  readonly homeworks = signal<Homework[]>([]);
  readonly showForm = signal(false);
  readonly saving = signal(false);

  filterDate = '';
  form: CreateHomeworkRequest = this.emptyForm();

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.list(1, 30, this.filterDate || undefined).subscribe({
      next: (r) => { this.homeworks.set(r.items ?? []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  isValid(): boolean {
    return !!this.form.subject.trim() && !!this.form.description.trim() && !!this.form.assignedDate;
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

  private emptyForm(): CreateHomeworkRequest {
    const today = new Date().toISOString().split('T')[0];
    return { subject: '', description: '', assignedDate: today };
  }
}
