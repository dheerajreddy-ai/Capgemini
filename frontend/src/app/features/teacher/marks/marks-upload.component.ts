import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TeacherService } from '../teacher.service';
import { ToastService } from '../../../core/services/toast.service';
import { ClassSection, TeacherStudent, ExamType } from '../../../core/models/models';

type MarksRow = TeacherStudent & {
  mathInput?: number;
  scienceInput?: number;
  englishInput?: number;
  teluguInput?: number;
  socialInput?: number;
};

@Component({
  selector: 'ev-marks-upload',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-header mb-4">
      <div>
        <h1 class="ev-page-title">Upload Marks</h1>
        <p class="ev-page-sub text-secondary-ev">Enter exam marks for your students</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="ev-card p-3 mb-4">
      <div class="row g-3 align-items-end">
        <div class="col-12 col-md-3">
          <label class="form-label">Class</label>
          <select class="form-select" [(ngModel)]="selectedClass" (change)="onClassChange()">
            <option value="">Select class…</option>
            @for (c of uniqueClasses(); track c) {
              <option [value]="c">Class {{ c }}</option>
            }
          </select>
        </div>
        <div class="col-12 col-md-2">
          <label class="form-label">Section</label>
          <select class="form-select" [(ngModel)]="selectedSection" [disabled]="!selectedClass">
            <option value="">All</option>
            @for (s of sectionsForClass(); track s) {
              <option [value]="s">{{ s }}</option>
            }
          </select>
        </div>
        <div class="col-12 col-md-2">
          <label class="form-label">Exam Type</label>
          <select class="form-select" [(ngModel)]="examType">
            @for (t of examTypes; track t) { <option [value]="t">{{ t }}</option> }
          </select>
        </div>
        <div class="col-12 col-md-2">
          <label class="form-label">Exam Date</label>
          <input type="date" class="form-control" [(ngModel)]="examDate" [max]="today" />
        </div>
        <div class="col-12 col-md-1">
          <label class="form-label">Max /subject</label>
          <input type="number" class="form-control" [(ngModel)]="maxMarks" min="1" max="500" />
        </div>
        <div class="col-12 col-md-2">
          <button class="btn btn-outline-primary w-100" (click)="load()" [disabled]="!selectedClass || loading()">
            <i class="bi bi-people me-1"></i> Load
          </button>
        </div>
      </div>
    </div>

    @if (loading()) {
      <div class="text-center py-4"><div class="spinner-border text-accent"></div></div>
    }

    @if (rows().length > 0) {
      <div class="ev-card p-3">
        <div class="d-flex align-items-center justify-content-between mb-3">
          <span class="fw-semibold">{{ rows().length }} students · Max {{ maxMarks }} marks per subject</span>
        </div>

        <div class="table-responsive">
          <table class="table ev-table align-middle">
            <thead>
              <tr>
                <th>#</th>
                <th>Student</th>
                <th>Math</th>
                <th>Science</th>
                <th>English</th>
                <th>Telugu</th>
                <th>Social</th>
                <th>% (preview)</th>
              </tr>
            </thead>
            <tbody>
              @for (row of rows(); track row.id; let i = $index) {
                <tr>
                  <td class="text-secondary-ev small">{{ i + 1 }}</td>
                  <td>
                    <div class="fw-semibold small">{{ row.fullName }}</div>
                    <div class="text-secondary-ev" style="font-size:.75rem">{{ row.studentCode }}</div>
                  </td>
                  <td><input type="number" class="form-control form-control-sm ev-marks-input" [(ngModel)]="row.mathInput" [min]="0" [max]="maxMarks" (ngModelChange)="rows.update(r => [...r])" /></td>
                  <td><input type="number" class="form-control form-control-sm ev-marks-input" [(ngModel)]="row.scienceInput" [min]="0" [max]="maxMarks" (ngModelChange)="rows.update(r => [...r])" /></td>
                  <td><input type="number" class="form-control form-control-sm ev-marks-input" [(ngModel)]="row.englishInput" [min]="0" [max]="maxMarks" (ngModelChange)="rows.update(r => [...r])" /></td>
                  <td><input type="number" class="form-control form-control-sm ev-marks-input" [(ngModel)]="row.teluguInput" [min]="0" [max]="maxMarks" (ngModelChange)="rows.update(r => [...r])" /></td>
                  <td><input type="number" class="form-control form-control-sm ev-marks-input" [(ngModel)]="row.socialInput" [min]="0" [max]="maxMarks" (ngModelChange)="rows.update(r => [...r])" /></td>
                  <td>
                    @if (previewPct(row) !== null) {
                      <span class="badge" [class]="pctBadge(previewPct(row)!)">{{ previewPct(row) }}%</span>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <div class="mt-3 d-flex justify-content-end">
          <button class="btn btn-primary px-4" (click)="submit()" [disabled]="saving()">
            @if (saving()) { <span class="spinner-border spinner-border-sm me-2"></span> }
            Save Marks
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .ev-card { background: var(--ev-bg-card); border-radius: 14px; border: 1px solid var(--ev-border); }
    .ev-marks-input { width: 70px; text-align: center; }
    .ev-table th { font-size: .8rem; text-transform: uppercase; letter-spacing: .04em; color: var(--ev-text-muted); border-color: var(--ev-border); }
    .ev-table td { border-color: var(--ev-border); }
  `],
})
export class MarksUploadComponent implements OnInit {
  private readonly service = inject(TeacherService);
  private readonly toast = inject(ToastService);

  readonly today = new Date().toISOString().split('T')[0];
  selectedClass = '';
  selectedSection = '';
  examType: ExamType = 'UnitTest';
  examDate = this.today;
  maxMarks = 100;

  readonly examTypes: ExamType[] = ['UnitTest', 'Midterm', 'Final', 'Quarterly', 'HalfYearly', 'Annual'];
  readonly classSections = signal<ClassSection[]>([]);
  readonly rows = signal<MarksRow[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly uniqueClasses = computed(() => [...new Set(this.classSections().map((c) => c.class))].sort());

  sectionsForClass(): string[] {
    return this.classSections()
      .filter((c) => c.class === this.selectedClass && c.section)
      .map((c) => c.section!);
  }

  ngOnInit(): void {
    this.service.getClassSections().subscribe({ next: (cs) => this.classSections.set(cs) });
  }

  onClassChange(): void {
    this.selectedSection = '';
    this.rows.set([]);
  }

  load(): void {
    if (!this.selectedClass) return;
    this.loading.set(true);
    this.service.getStudents(this.selectedClass, this.selectedSection || undefined).subscribe({
      next: (students) => {
        this.rows.set(students.map((s) => ({
          ...s,
          mathInput: s.mathMarks ?? undefined,
          scienceInput: s.scienceMarks ?? undefined,
          englishInput: s.englishMarks ?? undefined,
          teluguInput: s.teluguMarks ?? undefined,
          socialInput: s.socialMarks ?? undefined,
        })));
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  previewPct(row: MarksRow): number | null {
    const vals = [row.mathInput, row.scienceInput, row.englishInput, row.teluguInput, row.socialInput]
      .filter((v) => v !== null && v !== undefined) as number[];
    if (vals.length === 0) return null;
    return Math.round(vals.reduce((a, b) => a + b, 0) / (this.maxMarks * vals.length) * 100);
  }

  pctBadge(pct: number): string {
    if (pct >= 80) return 'bg-success';
    if (pct >= 60) return 'bg-primary';
    if (pct >= 40) return 'bg-warning text-dark';
    return 'bg-danger';
  }

  submit(): void {
    if (this.rows().length === 0) return;
    this.saving.set(true);
    this.service.uploadMarks({
      examType: this.examType,
      examDate: new Date(this.examDate).toISOString(),
      class: this.selectedClass,
      section: this.selectedSection || undefined,
      maxMarksPerSubject: this.maxMarks,
      entries: this.rows().map((r) => ({
        studentId: r.id,
        mathMarks: r.mathInput,
        scienceMarks: r.scienceInput,
        englishMarks: r.englishInput,
        teluguMarks: r.teluguInput,
        socialMarks: r.socialInput,
      })),
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Marks saved', `Marks uploaded for ${this.rows().length} students.`);
      },
      error: () => { this.saving.set(false); this.toast.error('Error', 'Failed to save marks.'); },
    });
  }
}
