import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DropoutRiskService } from '../../core/services/dropout-risk.service';
import { DropoutRiskSummary, DropoutRiskStudent, DropoutRiskLevel } from '../../core/models/models';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'ev-dropout-risk',
  standalone: true,
  imports: [FormsModule, DecimalPipe, RouterLink, SkeletonComponent, EmptyStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Dropout Risk</h1>
        <p class="ev-page-sub">AI-scored students at risk of dropping out — recalculated weekly</p>
      </div>
      <button class="btn btn-outline-primary" [disabled]="recalculating()" (click)="recalculate()">
        @if (recalculating()) { <span class="spinner-border spinner-border-sm me-2"></span> }
        <i class="bi bi-arrow-clockwise me-1"></i>Recalculate Now
      </button>
    </div>

    @if (loading()) {
      <div class="ev-card ev-card-pad"><ev-skeleton [rows]="4" [height]="20" /></div>
    } @else if (summary()) {
      <!-- Summary cards -->
      <div class="row g-3 mb-4">
        <div class="col-6 col-md-3">
          <div class="ev-stat-card border-0 shadow-sm text-center p-3 rounded-3" style="border-left:4px solid #dc3545!important">
            <div style="font-size:2rem;font-weight:700;color:#dc3545">{{ summary()!.criticalCount }}</div>
            <div class="text-muted small">Critical</div>
          </div>
        </div>
        <div class="col-6 col-md-3">
          <div class="ev-stat-card border-0 shadow-sm text-center p-3 rounded-3" style="border-left:4px solid #fd7e14!important">
            <div style="font-size:2rem;font-weight:700;color:#fd7e14">{{ summary()!.highCount }}</div>
            <div class="text-muted small">High</div>
          </div>
        </div>
        <div class="col-6 col-md-3">
          <div class="ev-stat-card border-0 shadow-sm text-center p-3 rounded-3" style="border-left:4px solid #ffc107!important">
            <div style="font-size:2rem;font-weight:700;color:#ffc107">{{ summary()!.mediumCount }}</div>
            <div class="text-muted small">Medium</div>
          </div>
        </div>
        <div class="col-6 col-md-3">
          <div class="ev-stat-card border-0 shadow-sm text-center p-3 rounded-3">
            <div style="font-size:2rem;font-weight:700" class="text-muted">{{ summary()!.lowCount }}</div>
            <div class="text-muted small">Low Risk</div>
          </div>
        </div>
      </div>

      <!-- Filter -->
      <div class="ev-card ev-card-pad mb-3 d-flex gap-3 align-items-center">
        <label class="form-label mb-0 fw-semibold">Filter</label>
        @for (level of levels; track level.value) {
          <div class="form-check mb-0">
            <input class="form-check-input" type="radio" name="riskFilter"
              [value]="level.value" [(ngModel)]="filterLevel" (change)="load()" />
            <label class="form-check-label">{{ level.label }}</label>
          </div>
        }
      </div>

      <!-- Table -->
      @if (summary()!.atRiskStudents.length === 0) {
        <div class="ev-card">
          <ev-empty-state icon="bi-shield-check" title="No at-risk students"
            message="All students are currently at low dropout risk." />
        </div>
      } @else {
        <div class="ev-card">
          <table class="table table-hover mb-0">
            <thead class="table-light">
              <tr>
                <th>Student</th>
                <th>Class</th>
                <th>Risk Score</th>
                <th>Attendance</th>
                <th>Academic %</th>
                <th>Fees</th>
                <th>No-Answer (30d)</th>
                <th>Risk Factors</th>
              </tr>
            </thead>
            <tbody>
              @for (s of summary()!.atRiskStudents; track s.studentId) {
                <tr>
                  <td>
                    <div class="fw-semibold">{{ s.studentName }}</div>
                    <div class="text-muted small">{{ s.parentPhone }}</div>
                  </td>
                  <td class="small">{{ s.class }}{{ s.section ? ' ' + s.section : '' }}</td>
                  <td>
                    <div class="d-flex align-items-center gap-2">
                      <div class="progress flex-grow-1" style="height:6px;width:60px">
                        <div class="progress-bar" [class]="riskBarClass(s.riskLevel)"
                          [style.width.%]="s.riskScore"></div>
                      </div>
                      <span class="fw-bold small" [class]="riskTextClass(s.riskLevel)">{{ s.riskScore }}</span>
                    </div>
                    <span class="badge mt-1" [class]="riskBadgeClass(s.riskLevel)">{{ s.riskLevel }}</span>
                  </td>
                  <td>
                    @if (s.attendancePercentage != null) {
                      <span [class.text-danger]="s.attendancePercentage < 65"
                            [class.text-warning]="s.attendancePercentage >= 65 && s.attendancePercentage < 75">
                        {{ s.attendancePercentage | number:'1.0-0' }}%
                      </span>
                    } @else { <span class="text-muted">—</span> }
                  </td>
                  <td>
                    @if (s.academicPercentage != null) {
                      <span [class.text-danger]="s.academicPercentage < 40">
                        {{ s.academicPercentage | number:'1.0-0' }}%
                      </span>
                    } @else { <span class="text-muted">—</span> }
                  </td>
                  <td>
                    <span class="badge"
                      [class.bg-danger]="s.feesStatus === 'Overdue'"
                      [class.bg-warning]="s.feesStatus === 'Unpaid'"
                      [class.bg-light]="s.feesStatus === 'Partial'"
                      [class.text-dark]="s.feesStatus === 'Partial' || s.feesStatus === 'Paid'">
                      {{ s.feesStatus }}
                    </span>
                    @if (s.pendingFees > 0) {
                      <div class="text-muted small">₹{{ s.pendingFees | number:'1.0-0' }}</div>
                    }
                  </td>
                  <td class="text-center">
                    @if (s.noAnswerCallsLast30Days >= 3) {
                      <span class="text-danger fw-bold">{{ s.noAnswerCallsLast30Days }}</span>
                    } @else {
                      {{ s.noAnswerCallsLast30Days }}
                    }
                  </td>
                  <td class="small">
                    @for (r of s.riskReasons; track r) {
                      <div class="text-muted"><i class="bi bi-dot"></i>{{ r }}</div>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    }
  `,
})
export class DropoutRiskComponent implements OnInit {
  private readonly service = inject(DropoutRiskService);

  readonly loading = signal(true);
  readonly recalculating = signal(false);
  readonly summary = signal<DropoutRiskSummary | null>(null);

  filterLevel = '';

  readonly levels = [
    { value: '', label: 'All at-risk' },
    { value: 'Critical', label: 'Critical' },
    { value: 'High', label: 'High' },
    { value: 'Medium', label: 'Medium' },
  ];

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getSummary(this.filterLevel || undefined).subscribe({
      next: (r) => { this.summary.set(r); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  recalculate(): void {
    this.recalculating.set(true);
    this.service.recalculate().subscribe({
      next: () => { this.recalculating.set(false); this.load(); },
      error: () => this.recalculating.set(false),
    });
  }

  riskBadgeClass(level: DropoutRiskLevel): string {
    return { Critical: 'bg-danger', High: 'bg-warning text-dark', Medium: 'bg-info text-dark', Low: 'bg-secondary' }[level];
  }

  riskBarClass(level: DropoutRiskLevel): string {
    return { Critical: 'bg-danger', High: 'bg-warning', Medium: 'bg-info', Low: 'bg-success' }[level];
  }

  riskTextClass(level: DropoutRiskLevel): string {
    return { Critical: 'text-danger', High: 'text-warning', Medium: 'text-info', Low: 'text-muted' }[level];
  }
}
