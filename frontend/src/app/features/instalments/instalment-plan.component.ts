import {
  Component, ChangeDetectionStrategy, inject, signal, input, output, OnChanges,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';
import { InstalmentService, CreateInstalmentPlanRequest } from '../../core/services/instalment.service';
import { InstalmentPlan, FeeInstalment } from '../../core/models/models';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'ev-instalment-plan',
  standalone: true,
  imports: [FormsModule, DatePipe, DecimalPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="modal d-block" tabindex="-1" style="background:rgba(0,0,0,.5)">
      <div class="modal-dialog modal-lg modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="bi bi-calendar2-range me-2 text-primary"></i>
              Instalment Plan — {{ studentName() }}
            </h5>
            <button type="button" class="btn-close" (click)="closed.emit()"></button>
          </div>

          <div class="modal-body">
            @if (loading()) {
              <div class="text-center py-4">
                <span class="spinner-border text-primary"></span>
              </div>
            } @else {

              <!-- Existing plan -->
              @if (plan() && plan()!.instalments.length > 0) {
                <div class="d-flex justify-content-between align-items-center mb-3">
                  <div>
                    <span class="badge bg-primary me-2">{{ plan()!.paidInstalments }}/{{ plan()!.totalInstalments }} paid</span>
                    @if (plan()!.overdueInstalments > 0) {
                      <span class="badge bg-danger">{{ plan()!.overdueInstalments }} overdue</span>
                    }
                  </div>
                  @if (hasUnpaid()) {
                    <button class="btn btn-sm btn-outline-danger" [disabled]="saving()" (click)="deletePlan()">
                      <i class="bi bi-trash me-1"></i>Remove Plan
                    </button>
                  }
                </div>

                <div class="table-responsive">
                  <table class="table table-hover align-middle mb-0">
                    <thead class="table-light">
                      <tr>
                        <th>#</th>
                        <th>Amount</th>
                        <th>Due Date</th>
                        <th>Status</th>
                        <th></th>
                      </tr>
                    </thead>
                    <tbody>
                      @for (inst of plan()!.instalments; track inst.id) {
                        <tr [class.table-success]="inst.isPaid"
                            [class.table-danger]="inst.isOverdue">
                          <td class="fw-semibold">{{ inst.instalmentNumber }}</td>
                          <td>₹{{ inst.amount | number:'1.0-0' }}</td>
                          <td>{{ inst.dueDate | date:'dd MMM yyyy' }}</td>
                          <td>
                            @if (inst.isPaid) {
                              <span class="badge bg-success">
                                <i class="bi bi-check-circle me-1"></i>Paid
                              </span>
                              <div class="text-muted" style="font-size:.7rem">{{ inst.paidAt | date:'dd MMM' }}</div>
                            } @else if (inst.isOverdue) {
                              <span class="badge bg-danger">{{ inst.daysOverdue }}d overdue</span>
                            } @else {
                              <span class="badge bg-secondary">Pending</span>
                            }
                          </td>
                          <td>
                            @if (!inst.isPaid) {
                              <button class="btn btn-sm btn-success"
                                [disabled]="saving()"
                                (click)="markPaid(inst)">
                                @if (markingId() === inst.id) {
                                  <span class="spinner-border spinner-border-sm"></span>
                                } @else {
                                  <i class="bi bi-check-lg me-1"></i>Mark Paid
                                }
                              </button>
                            }
                          </td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>

                @if (hasUnpaid()) {
                  <hr class="my-4" />
                  <p class="text-muted small mb-3">Replace the current plan with new settings:</p>
                }
              }

              <!-- Create / replace form -->
              @if (!plan() || !plan()!.instalments.length || hasUnpaid()) {
                <form class="row g-3" (ngSubmit)="createPlan()">
                  <div class="col-md-4">
                    <label class="form-label fw-semibold">Number of Instalments</label>
                    <select class="form-select" [(ngModel)]="form.instalmentCount" name="count">
                      @for (n of countOptions; track n) {
                        <option [value]="n">{{ n }}</option>
                      }
                    </select>
                  </div>
                  <div class="col-md-4">
                    <label class="form-label fw-semibold">First Due Date</label>
                    <input type="date" class="form-control" [(ngModel)]="form.firstDueDate"
                      name="firstDue" [min]="today" required />
                  </div>
                  <div class="col-md-4">
                    <label class="form-label fw-semibold">Interval</label>
                    <select class="form-select" [(ngModel)]="form.intervalDays" name="interval">
                      <option [value]="7">Weekly (7 days)</option>
                      <option [value]="14">Bi-weekly (14 days)</option>
                      <option [value]="30">Monthly (30 days)</option>
                      <option [value]="45">45 days</option>
                      <option [value]="60">Bi-monthly (60 days)</option>
                    </select>
                  </div>

                  <div class="col-12">
                    <div class="alert alert-info py-2 mb-0 small">
                      <i class="bi bi-info-circle me-1"></i>
                      Pending fees of <strong>₹{{ pendingFees() | number:'1.0-0' }}</strong> will be split into
                      <strong>{{ form.instalmentCount }}</strong> instalments of approx.
                      <strong>₹{{ approxAmount() | number:'1.0-0' }}</strong> each,
                      starting <strong>{{ form.firstDueDate | date:'dd MMM yyyy' }}</strong>.
                    </div>
                  </div>

                  <div class="col-12 d-flex gap-2 justify-content-end">
                    <button type="button" class="btn btn-outline-secondary" (click)="closed.emit()">Cancel</button>
                    <button type="submit" class="btn btn-primary" [disabled]="saving() || !form.firstDueDate">
                      @if (saving()) { <span class="spinner-border spinner-border-sm me-2"></span> }
                      <i class="bi bi-calendar-plus me-1"></i>
                      {{ plan() && plan()!.instalments.length ? 'Replace Plan' : 'Create Plan' }}
                    </button>
                  </div>
                </form>
              }
            }
          </div>
        </div>
      </div>
    </div>
  `,
})
export class InstalmentPlanComponent implements OnChanges {
  private readonly service = inject(InstalmentService);
  private readonly toast = inject(ToastService);

  readonly studentId = input.required<string>();
  readonly studentName = input<string>('');
  readonly pendingFees = input<number>(0);
  readonly closed = output<void>();
  readonly updated = output<void>();

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly markingId = signal<string | null>(null);
  readonly plan = signal<InstalmentPlan | null>(null);

  readonly today = new Date().toISOString().split('T')[0];
  readonly countOptions = [2, 3, 4, 5, 6, 8, 10, 12];

  form: CreateInstalmentPlanRequest = {
    instalmentCount: 3,
    firstDueDate: '',
    intervalDays: 30,
  };

  ngOnChanges(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getPlan(this.studentId()).subscribe({
      next: (p) => { this.plan.set(p); this.loading.set(false); },
      error: () => { this.plan.set(null); this.loading.set(false); },
    });
  }

  hasUnpaid(): boolean {
    return (this.plan()?.instalments ?? []).some((i) => !i.isPaid);
  }

  approxAmount(): number {
    return this.form.instalmentCount > 0
      ? Math.round(this.pendingFees() / this.form.instalmentCount)
      : 0;
  }

  createPlan(): void {
    if (!this.form.firstDueDate) return;
    this.saving.set(true);
    this.service.createPlan(this.studentId(), this.form).subscribe({
      next: (p) => {
        this.plan.set(p);
        this.saving.set(false);
        this.toast.success('Instalment plan created');
        this.updated.emit();
      },
      error: (err) => {
        this.toast.error(err?.error?.message ?? 'Failed to create plan');
        this.saving.set(false);
      },
    });
  }

  markPaid(inst: FeeInstalment): void {
    this.markingId.set(inst.id);
    this.saving.set(true);
    this.service.markPaid(this.studentId(), inst.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.markingId.set(null);
        this.toast.success(`Instalment #${inst.instalmentNumber} marked paid`);
        this.load();
        this.updated.emit();
      },
      error: (err) => {
        this.toast.error(err?.error?.message ?? 'Failed to mark paid');
        this.saving.set(false);
        this.markingId.set(null);
      },
    });
  }

  deletePlan(): void {
    if (!confirm('Remove all unpaid instalments? This cannot be undone.')) return;
    this.saving.set(true);
    this.service.deletePlan(this.studentId()).subscribe({
      next: () => {
        this.plan.set(null);
        this.saving.set(false);
        this.toast.success('Instalment plan removed');
        this.updated.emit();
      },
      error: () => {
        this.toast.error('Failed to remove plan');
        this.saving.set(false);
      },
    });
  }
}
