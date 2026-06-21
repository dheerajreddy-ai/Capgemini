import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { DecimalPipe, DatePipe, NgStyle } from '@angular/common';
import { FeeCollectionService } from '../../core/services/fee-collection.service';
import {
  FeeCollectionDashboard,
  ClassCollectionStat,
  DefaulterStudent,
  DailyRevenue,
} from '../../core/models/models';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'ev-fee-collection',
  standalone: true,
  imports: [DecimalPipe, DatePipe, NgStyle, SkeletonComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Fee Collection</h1>
        <p class="ev-page-sub">School-wide fee collection overview, defaulter tracking and daily revenue</p>
      </div>
      <button class="btn btn-outline-primary btn-sm" (click)="load()">
        <i class="bi bi-arrow-clockwise me-1"></i>Refresh
      </button>
    </div>

    @if (loading()) {
      <div class="ev-card ev-card-pad"><ev-skeleton [rows]="6" [height]="20" /></div>
    } @else if (data()) {
      <!-- KPI Row -->
      <div class="row g-3 mb-4">
        <div class="col-6 col-lg-3">
          <div class="ev-stat-card p-3 rounded-3 shadow-sm text-center border-0" style="border-top:4px solid #0d6efd!important">
            <div class="text-muted small mb-1">Total Expected</div>
            <div class="fw-bold fs-4">₹{{ data()!.totalFeesExpected | number:'1.0-0' }}</div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="ev-stat-card p-3 rounded-3 shadow-sm text-center border-0" style="border-top:4px solid #198754!important">
            <div class="text-muted small mb-1">Collected</div>
            <div class="fw-bold fs-4 text-success">₹{{ data()!.totalFeesCollected | number:'1.0-0' }}</div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="ev-stat-card p-3 rounded-3 shadow-sm text-center border-0" style="border-top:4px solid #dc3545!important">
            <div class="text-muted small mb-1">Pending</div>
            <div class="fw-bold fs-4 text-danger">₹{{ data()!.totalFeesPending | number:'1.0-0' }}</div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="ev-stat-card p-3 rounded-3 shadow-sm text-center border-0" style="border-top:4px solid #0dcaf0!important">
            <div class="text-muted small mb-1">Collection Rate</div>
            <div class="fw-bold fs-4">{{ data()!.collectionRatePercent | number:'1.1-1' }}%</div>
          </div>
        </div>
      </div>

      <!-- This Month vs Last Month -->
      <div class="row g-3 mb-4">
        <div class="col-md-6">
          <div class="ev-card ev-card-pad h-100">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <span class="fw-semibold">This Month</span>
              <span class="badge" [class.bg-success]="data()!.monthOnMonthChange >= 0"
                [class.bg-danger]="data()!.monthOnMonthChange < 0">
                {{ data()!.monthOnMonthChange >= 0 ? '▲' : '▼' }}
                {{ data()!.monthOnMonthChange | number:'1.1-1' }}% MoM
              </span>
            </div>
            <div class="display-6 fw-bold text-primary">₹{{ data()!.collectedThisMonth | number:'1.0-0' }}</div>
            <div class="text-muted small mt-1">Last month: ₹{{ data()!.collectedLastMonth | number:'1.0-0' }}</div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="ev-card ev-card-pad h-100">
            <div class="fw-semibold mb-3">Student Payment Status</div>
            <div class="d-flex justify-content-around text-center">
              <div>
                <div class="fw-bold text-success fs-5">{{ data()!.paidCount }}</div>
                <div class="text-muted small">Paid</div>
              </div>
              <div>
                <div class="fw-bold text-warning fs-5">{{ data()!.partialCount }}</div>
                <div class="text-muted small">Partial</div>
              </div>
              <div>
                <div class="fw-bold text-secondary fs-5">{{ data()!.unpaidCount }}</div>
                <div class="text-muted small">Unpaid</div>
              </div>
              <div>
                <div class="fw-bold text-danger fs-5">{{ data()!.overdueCount }}</div>
                <div class="text-muted small">Overdue</div>
              </div>
            </div>
            <div class="progress mt-3" style="height:8px">
              <div class="progress-bar bg-success"
                [style.width.%]="pct(data()!.paidCount, data()!.totalStudents)"></div>
              <div class="progress-bar bg-warning"
                [style.width.%]="pct(data()!.partialCount, data()!.totalStudents)"></div>
              <div class="progress-bar bg-secondary"
                [style.width.%]="pct(data()!.unpaidCount, data()!.totalStudents)"></div>
              <div class="progress-bar bg-danger"
                [style.width.%]="pct(data()!.overdueCount, data()!.totalStudents)"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Class-wise Breakdown -->
      <div class="ev-card mb-4">
        <div class="ev-card-head">Collection by Class</div>
        <div class="table-responsive">
          <table class="table table-hover mb-0">
            <thead class="table-light">
              <tr>
                <th>Class</th>
                <th>Students</th>
                <th>Expected</th>
                <th>Collected</th>
                <th>Rate</th>
                <th>Progress</th>
              </tr>
            </thead>
            <tbody>
              @for (cls of data()!.byClass; track cls.class) {
                <tr>
                  <td class="fw-semibold">{{ cls.class }}</td>
                  <td>{{ cls.studentCount }}</td>
                  <td>₹{{ cls.totalExpected | number:'1.0-0' }}</td>
                  <td class="text-success">₹{{ cls.totalCollected | number:'1.0-0' }}</td>
                  <td>
                    <span [class.text-success]="cls.collectionRatePercent >= 80"
                          [class.text-warning]="cls.collectionRatePercent >= 50 && cls.collectionRatePercent < 80"
                          [class.text-danger]="cls.collectionRatePercent < 50">
                      {{ cls.collectionRatePercent | number:'1.1-1' }}%
                    </span>
                  </td>
                  <td style="min-width:100px">
                    <div class="progress" style="height:6px">
                      <div class="progress-bar"
                        [class.bg-success]="cls.collectionRatePercent >= 80"
                        [class.bg-warning]="cls.collectionRatePercent >= 50 && cls.collectionRatePercent < 80"
                        [class.bg-danger]="cls.collectionRatePercent < 50"
                        [style.width.%]="cls.collectionRatePercent"></div>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>

      <div class="row g-3">
        <!-- Top Defaulters -->
        <div class="col-lg-7">
          <div class="ev-card h-100">
            <div class="ev-card-head">Top 10 Defaulters</div>
            @if (data()!.topDefaulters.length === 0) {
              <div class="p-4 text-muted text-center">No outstanding defaulters</div>
            } @else {
              <div class="table-responsive">
                <table class="table table-hover mb-0">
                  <thead class="table-light">
                    <tr>
                      <th>Student</th>
                      <th>Class</th>
                      <th>Pending</th>
                      <th>Days Overdue</th>
                      <th>Escalation</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (d of data()!.topDefaulters; track d.studentId) {
                      <tr>
                        <td>
                          <div class="fw-semibold small">{{ d.studentName }}</div>
                          <div class="text-muted" style="font-size:.75rem">{{ d.parentPhone }}</div>
                        </td>
                        <td class="small">{{ d.class }}{{ d.section ? ' ' + d.section : '' }}</td>
                        <td class="text-danger fw-bold small">₹{{ d.pendingFees | number:'1.0-0' }}</td>
                        <td>
                          @if (d.daysOverdue > 0) {
                            <span class="badge"
                              [class.bg-danger]="d.daysOverdue >= 60"
                              [class.bg-warning]="d.daysOverdue >= 30 && d.daysOverdue < 60"
                              [class.bg-secondary]="d.daysOverdue < 30">
                              {{ d.daysOverdue }}d
                            </span>
                          } @else {
                            <span class="text-muted small">—</span>
                          }
                        </td>
                        <td>
                          <span class="badge"
                            [class.bg-danger]="d.defaulterEscalationLevel === 'Day90'"
                            [class.bg-warning]="d.defaulterEscalationLevel === 'Day60'"
                            [class.bg-info]="d.defaulterEscalationLevel === 'Day30'"
                            [class.bg-secondary]="d.defaulterEscalationLevel === 'None'">
                            {{ d.defaulterEscalationLevel }}
                          </span>
                        </td>
                        <td>
                          @if (d.needsPersonalFollowup) {
                            <span class="badge bg-danger">Follow-up</span>
                          }
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>

        <!-- Daily Revenue (last 30 days mini chart) -->
        <div class="col-lg-5">
          <div class="ev-card h-100">
            <div class="ev-card-head">Daily Revenue (Last 30 Days)</div>
            <div class="p-3">
              @if (maxDailyRevenue() === 0) {
                <div class="text-muted text-center py-4">No payment data for this period</div>
              } @else {
                <div class="d-flex align-items-end gap-0" style="height:120px;overflow-x:auto">
                  @for (day of data()!.dailyRevenue; track day.date) {
                    <div class="d-flex flex-column align-items-center flex-grow-1" style="min-width:4px"
                      [title]="day.label + ': ₹' + (day.amount | number:'1.0-0')">
                      <div class="bg-primary rounded-top"
                        [style.height.%]="barHeight(day.amount)"
                        style="width:80%;min-height:1px;transition:height .2s"></div>
                    </div>
                  }
                </div>
                <div class="d-flex justify-content-between text-muted mt-1" style="font-size:.65rem">
                  <span>{{ data()!.dailyRevenue[0]?.label }}</span>
                  <span>{{ data()!.dailyRevenue[data()!.dailyRevenue.length - 1]?.label }}</span>
                </div>
                <div class="mt-3 text-muted small text-center">
                  Peak: ₹{{ maxDailyRevenue() | number:'1.0-0' }}
                </div>
              }
            </div>
          </div>
        </div>
      </div>
    }
  `,
})
export class FeeCollectionComponent implements OnInit {
  private readonly service = inject(FeeCollectionService);

  readonly loading = signal(true);
  readonly data = signal<FeeCollectionDashboard | null>(null);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getDashboard().subscribe({
      next: (d) => { this.data.set(d); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  pct(part: number, total: number): number {
    return total > 0 ? (part / total) * 100 : 0;
  }

  maxDailyRevenue(): number {
    const rev = this.data()?.dailyRevenue ?? [];
    return rev.length > 0 ? Math.max(...rev.map((r) => r.amount)) : 0;
  }

  barHeight(amount: number): number {
    const max = this.maxDailyRevenue();
    return max > 0 ? Math.max(2, (amount / max) * 100) : 2;
  }
}
