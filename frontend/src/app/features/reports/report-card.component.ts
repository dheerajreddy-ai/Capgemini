import {
  Component, ChangeDetectionStrategy, inject, signal, input, output, OnChanges,
} from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { ReportsService } from '../../core/services/reports.service';
import { StudentReportCard } from '../../core/models/models';

@Component({
  selector: 'ev-report-card',
  standalone: true,
  imports: [DecimalPipe, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    @media print {
      .no-print { display: none !important; }
      .modal-header, .modal-footer { display: none !important; }
      .modal { position: static !important; background: none !important; }
      .modal-dialog { max-width: 100% !important; margin: 0 !important; }
      .modal-content { border: none !important; box-shadow: none !important; }
      .modal-body { padding: 0 !important; }
    }
    .subject-row td { padding: .35rem .5rem; }
  `],
  template: `
    <div class="modal d-block" tabindex="-1" style="background:rgba(0,0,0,.5)">
      <div class="modal-dialog modal-lg modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header no-print">
            <h5 class="modal-title"><i class="bi bi-file-earmark-text me-2 text-primary"></i>Report Card</h5>
            <div class="d-flex gap-2">
              <button class="btn btn-sm btn-outline-primary" (click)="print()">
                <i class="bi bi-printer me-1"></i>Print
              </button>
              <button type="button" class="btn-close" (click)="closed.emit()"></button>
            </div>
          </div>

          <div class="modal-body">
            @if (loading()) {
              <div class="text-center py-5">
                <span class="spinner-border text-primary"></span>
              </div>
            } @else if (report()) {
              <div id="report-card-print" class="p-3" style="font-family:'Segoe UI',sans-serif">

                <!-- School header -->
                <div class="text-center border-bottom pb-3 mb-3">
                  @if (report()!.schoolLogoUrl) {
                    <img [src]="report()!.schoolLogoUrl" alt="Logo" style="height:60px;object-fit:contain;margin-bottom:.5rem" />
                  }
                  <h4 class="mb-0 fw-bold">{{ report()!.schoolName }}</h4>
                  @if (report()!.schoolAddress) {
                    <div class="text-muted small">{{ report()!.schoolAddress }}</div>
                  }
                  <div class="fw-semibold mt-2" style="font-size:1.1rem;letter-spacing:.5px">STUDENT PROGRESS REPORT</div>
                </div>

                <!-- Student info -->
                <div class="row g-2 mb-4 p-2 rounded" style="background:#f8f9fa">
                  <div class="col-6">
                    <span class="text-muted small">Student Name</span>
                    <div class="fw-bold">{{ report()!.studentName }}</div>
                  </div>
                  <div class="col-3">
                    <span class="text-muted small">Class</span>
                    <div class="fw-bold">{{ report()!.class ?? '—' }}{{ report()!.section ? ' – ' + report()!.section : '' }}</div>
                  </div>
                  <div class="col-3">
                    <span class="text-muted small">Roll No.</span>
                    <div class="fw-bold">{{ report()!.studentCode }}</div>
                  </div>
                  <div class="col-6">
                    <span class="text-muted small">Parent / Guardian</span>
                    <div class="fw-bold">{{ report()!.parentName }}</div>
                  </div>
                  <div class="col-3">
                    <span class="text-muted small">Phone</span>
                    <div class="fw-bold">{{ report()!.parentPhone }}</div>
                  </div>
                  @if (report()!.dateOfBirth) {
                    <div class="col-3">
                      <span class="text-muted small">Date of Birth</span>
                      <div class="fw-bold">{{ report()!.dateOfBirth | date:'dd MMM yyyy' }}</div>
                    </div>
                  }
                </div>

                <!-- Marks table -->
                <h6 class="fw-bold mb-2"><i class="bi bi-book me-1"></i>Academic Performance</h6>
                <table class="table table-bordered mb-4" style="font-size:.9rem">
                  <thead style="background:#e9ecef">
                    <tr>
                      <th>Subject</th>
                      <th class="text-center">Marks Obtained</th>
                      <th class="text-center">Max Marks</th>
                      <th class="text-center">%</th>
                      <th class="text-center">Result</th>
                    </tr>
                  </thead>
                  <tbody class="subject-row">
                    @for (sub of subjects(); track sub.label) {
                      <tr>
                        <td>{{ sub.label }}</td>
                        <td class="text-center">{{ sub.marks != null ? (sub.marks | number:'1.0-1') : '—' }}</td>
                        <td class="text-center">{{ report()!.maxMarks != null ? (report()!.maxMarks! / 5 | number:'1.0-0') : '—' }}</td>
                        <td class="text-center">
                          @if (sub.marks != null && report()!.maxMarks) {
                            {{ (sub.marks / (report()!.maxMarks! / 5) * 100) | number:'1.0-0' }}%
                          } @else { — }
                        </td>
                        <td class="text-center">
                          @if (sub.marks != null && report()!.maxMarks) {
                            @if ((sub.marks / (report()!.maxMarks! / 5) * 100) >= 35) {
                              <span class="text-success fw-semibold">Pass</span>
                            } @else {
                              <span class="text-danger fw-semibold">Fail</span>
                            }
                          } @else { — }
                        </td>
                      </tr>
                    }
                  </tbody>
                  <tfoot style="background:#e9ecef;font-weight:600">
                    <tr>
                      <td>TOTAL</td>
                      <td class="text-center">{{ report()!.totalMarks != null ? (report()!.totalMarks | number:'1.0-0') : '—' }}</td>
                      <td class="text-center">{{ report()!.maxMarks != null ? (report()!.maxMarks | number:'1.0-0') : '—' }}</td>
                      <td class="text-center">{{ report()!.percentage != null ? (report()!.percentage | number:'1.1-1') + '%' : '—' }}</td>
                      <td class="text-center">
                        @if (report()!.grade) {
                          <span class="badge bg-primary">{{ report()!.grade }}</span>
                        } @else { — }
                      </td>
                    </tr>
                  </tfoot>
                </table>

                <!-- Attendance & Fees side by side -->
                <div class="row g-3 mb-4">
                  <div class="col-md-6">
                    <h6 class="fw-bold mb-2"><i class="bi bi-calendar-check me-1"></i>Attendance</h6>
                    <table class="table table-bordered mb-0" style="font-size:.9rem">
                      <tbody>
                        <tr>
                          <td class="text-muted">Days Present</td>
                          <td class="fw-semibold">{{ report()!.attendancePresentDays ?? '—' }}</td>
                        </tr>
                        <tr>
                          <td class="text-muted">Total Working Days</td>
                          <td class="fw-semibold">{{ report()!.attendanceTotalDays ?? '—' }}</td>
                        </tr>
                        <tr>
                          <td class="text-muted">Attendance %</td>
                          <td class="fw-semibold"
                            [class.text-success]="(report()!.attendancePercentage ?? 0) >= 75"
                            [class.text-danger]="(report()!.attendancePercentage ?? 0) < 65">
                            {{ report()!.attendancePercentage != null ? (report()!.attendancePercentage | number:'1.1-1') + '%' : '—' }}
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                  <div class="col-md-6">
                    <h6 class="fw-bold mb-2"><i class="bi bi-cash-coin me-1"></i>Fee Status</h6>
                    <table class="table table-bordered mb-0" style="font-size:.9rem">
                      <tbody>
                        <tr>
                          <td class="text-muted">Total Fees</td>
                          <td class="fw-semibold">₹{{ report()!.totalFees | number:'1.0-0' }}</td>
                        </tr>
                        <tr>
                          <td class="text-muted">Paid</td>
                          <td class="fw-semibold text-success">₹{{ report()!.paidFees | number:'1.0-0' }}</td>
                        </tr>
                        <tr>
                          <td class="text-muted">Balance</td>
                          <td class="fw-semibold"
                            [class.text-danger]="report()!.pendingFees > 0">
                            ₹{{ report()!.pendingFees | number:'1.0-0' }}
                            <span class="ms-1 badge"
                              [class.bg-success]="report()!.feesStatus === 'Paid'"
                              [class.bg-warning]="report()!.feesStatus === 'Partial'"
                              [class.bg-danger]="report()!.feesStatus === 'Overdue' || report()!.feesStatus === 'Unpaid'">
                              {{ report()!.feesStatus }}
                            </span>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>

                <!-- Remarks -->
                @if (report()!.remarks) {
                  <div class="p-3 rounded border mb-4" style="background:#fffbf0">
                    <span class="fw-semibold">Teacher's Remarks: </span>{{ report()!.remarks }}
                  </div>
                }

                <!-- Footer -->
                <div class="d-flex justify-content-between mt-4 pt-3 border-top text-muted small">
                  <span>Generated on {{ report()!.generatedAt | date:'dd MMM yyyy, hh:mm a' }}</span>
                  <span>— EduVoice</span>
                </div>
              </div>
            }
          </div>
        </div>
      </div>
    </div>
  `,
})
export class ReportCardComponent implements OnChanges {
  private readonly service = inject(ReportsService);

  readonly studentId = input.required<string>();
  readonly closed = output<void>();

  readonly loading = signal(true);
  readonly report = signal<StudentReportCard | null>(null);

  ngOnChanges(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getStudentReportCard(this.studentId()).subscribe({
      next: (r) => { this.report.set(r); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  subjects(): { label: string; marks: number | undefined }[] {
    const r = this.report();
    if (!r) return [];
    return [
      { label: 'Mathematics', marks: r.mathMarks },
      { label: 'Science', marks: r.scienceMarks },
      { label: 'English', marks: r.englishMarks },
      { label: 'Telugu', marks: r.teluguMarks },
      { label: 'Social Studies', marks: r.socialMarks },
    ];
  }

  print(): void { window.print(); }
}
