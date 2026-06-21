import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportsService } from '../../core/services/reports.service';
import { AcademicReport, ClassPerformance, TopPerformer } from '../../core/models/models';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'ev-reports',
  standalone: true,
  imports: [DecimalPipe, FormsModule, SkeletonComponent, EmptyStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Academic Reports</h1>
        <p class="ev-page-sub">Class-level performance analytics and top performers</p>
      </div>
      <div class="d-flex gap-2 align-items-center">
        <input class="form-control form-control-sm" style="width:120px" placeholder="Filter class"
          [(ngModel)]="filterClass" (change)="load()" />
        <button class="btn btn-outline-secondary btn-sm" (click)="filterClass=''; load()">
          <i class="bi bi-x-circle me-1"></i>Clear
        </button>
      </div>
    </div>

    @if (loading()) {
      <div class="ev-card ev-card-pad"><ev-skeleton [rows]="6" [height]="20" /></div>
    } @else if (!report() || report()!.byClass.length === 0) {
      <div class="ev-card">
        <ev-empty-state icon="bi-bar-chart" title="No academic data"
          message="Update student marks to see class performance analytics." />
      </div>
    } @else {

      <!-- Class performance table -->
      <div class="ev-card mb-4">
        <div class="ev-card-head">Class Performance Overview</div>
        <div class="table-responsive">
          <table class="table table-hover align-middle mb-0">
            <thead class="table-light">
              <tr>
                <th>Class</th>
                <th>Students</th>
                <th>Avg %</th>
                <th>Highest</th>
                <th>Lowest</th>
                <th>Pass Rate</th>
                <th>Maths Avg</th>
                <th>Science Avg</th>
                <th>English Avg</th>
                <th>Telugu Avg</th>
                <th>Social Avg</th>
              </tr>
            </thead>
            <tbody>
              @for (cls of report()!.byClass; track cls.class) {
                <tr>
                  <td class="fw-semibold">{{ cls.class }}</td>
                  <td>{{ cls.totalStudents }}</td>
                  <td>
                    <span [class.text-success]="(cls.averagePercentage ?? 0) >= 75"
                          [class.text-warning]="(cls.averagePercentage ?? 0) >= 50 && (cls.averagePercentage ?? 0) < 75"
                          [class.text-danger]="(cls.averagePercentage ?? 0) < 50">
                      {{ cls.averagePercentage != null ? (cls.averagePercentage | number:'1.1-1') + '%' : '—' }}
                    </span>
                  </td>
                  <td class="text-success fw-semibold">
                    {{ cls.highestPercentage != null ? (cls.highestPercentage | number:'1.1-1') + '%' : '—' }}
                  </td>
                  <td class="text-danger">
                    {{ cls.lowestPercentage != null ? (cls.lowestPercentage | number:'1.1-1') + '%' : '—' }}
                  </td>
                  <td>
                    <div class="d-flex align-items-center gap-2">
                      <div class="progress flex-grow-1" style="height:6px;min-width:60px">
                        <div class="progress-bar"
                          [class.bg-success]="cls.passPercent >= 80"
                          [class.bg-warning]="cls.passPercent >= 60 && cls.passPercent < 80"
                          [class.bg-danger]="cls.passPercent < 60"
                          [style.width.%]="cls.passPercent"></div>
                      </div>
                      <span class="small">{{ cls.passPercent | number:'1.0-0' }}%</span>
                    </div>
                  </td>
                  @for (sub of cls.subjectStats; track sub.subject) {
                    <td class="text-center small">
                      {{ sub.averageMarks != null ? (sub.averageMarks | number:'1.0-1') : '—' }}
                    </td>
                  }
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>

      <!-- Subject drill-down cards -->
      <div class="row g-3 mb-4">
        @for (subject of subjects(); track subject) {
          <div class="col-md-4">
            <div class="ev-card p-3">
              <div class="fw-semibold mb-2">{{ subject }}</div>
              <div class="d-flex flex-column gap-1">
                @for (cls of report()!.byClass; track cls.class) {
                  @let stat = subjectStat(cls, subject);
                  @if (stat && stat.averageMarks != null) {
                    <div class="d-flex align-items-center gap-2 small">
                      <span style="min-width:40px" class="text-muted">{{ cls.class }}</span>
                      <div class="progress flex-grow-1" style="height:8px">
                        <div class="progress-bar bg-primary"
                          [style.width.%]="stat.averageMarks"></div>
                      </div>
                      <span style="min-width:32px" class="text-end">{{ stat.averageMarks | number:'1.0-0' }}</span>
                    </div>
                  }
                }
              </div>
            </div>
          </div>
        }
      </div>

      <!-- Top performers -->
      @if (report()!.topPerformers.length > 0) {
        <div class="ev-card">
          <div class="ev-card-head"><i class="bi bi-trophy me-1 text-warning"></i>Top 10 Performers</div>
          <div class="table-responsive">
            <table class="table table-hover mb-0 align-middle">
              <thead class="table-light">
                <tr>
                  <th>#</th>
                  <th>Student</th>
                  <th>Class</th>
                  <th>Percentage</th>
                  <th>Grade</th>
                </tr>
              </thead>
              <tbody>
                @for (p of report()!.topPerformers; track p.studentId; let i = $index) {
                  <tr>
                    <td>
                      @if (i === 0) { <i class="bi bi-trophy-fill text-warning fs-5"></i> }
                      @else if (i === 1) { <i class="bi bi-trophy-fill text-secondary fs-5"></i> }
                      @else if (i === 2) { <i class="bi bi-trophy-fill" style="color:#cd7f32;font-size:1.1rem"></i> }
                      @else { <span class="text-muted">{{ i + 1 }}</span> }
                    </td>
                    <td class="fw-semibold">{{ p.studentName }}</td>
                    <td class="text-muted small">{{ p.class }}{{ p.section ? ' – ' + p.section : '' }}</td>
                    <td>
                      <span class="fw-bold text-success">{{ p.percentage | number:'1.1-1' }}%</span>
                    </td>
                    <td>
                      @if (p.grade) {
                        <span class="badge bg-primary">{{ p.grade }}</span>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }
    }
  `,
})
export class ReportsComponent implements OnInit {
  private readonly service = inject(ReportsService);

  readonly loading = signal(true);
  readonly report = signal<AcademicReport | null>(null);
  filterClass = '';

  readonly subjects = () => ['Maths', 'Science', 'English', 'Telugu', 'Social'];

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getAcademicReport(this.filterClass || undefined).subscribe({
      next: (r) => { this.report.set(r); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  subjectStat(cls: ClassPerformance, subject: string) {
    const map: Record<string, string> = {
      Maths: 'Maths', Science: 'Science', English: 'English', Telugu: 'Telugu', Social: 'Social',
    };
    return cls.subjectStats.find((s) => s.subject === map[subject]);
  }
}
