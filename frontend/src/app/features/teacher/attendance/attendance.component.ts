import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TeacherService } from '../teacher.service';
import { ToastService } from '../../../core/services/toast.service';
import { ClassSection, AttendanceResult, AttendanceEntry } from '../../../core/models/models';

@Component({
  selector: 'ev-attendance',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-header mb-4">
      <div>
        <h1 class="ev-page-title">Mark Attendance</h1>
        <p class="ev-page-sub text-secondary-ev">Mark daily attendance for your class</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="ev-card p-3 mb-4">
      <div class="row g-3 align-items-end">
        <div class="col-12 col-md-3">
          <label class="form-label">Date</label>
          <input type="date" class="form-control" [(ngModel)]="selectedDate" (change)="onFilterChange()" [max]="today" />
        </div>
        <div class="col-12 col-md-3">
          <label class="form-label">Class</label>
          <select class="form-select" [(ngModel)]="selectedClass" (change)="onClassChange()">
            <option value="">Select class…</option>
            @for (c of uniqueClasses(); track c) {
              <option [value]="c">Class {{ c }}</option>
            }
          </select>
        </div>
        <div class="col-12 col-md-3">
          <label class="form-label">Section</label>
          <select class="form-select" [(ngModel)]="selectedSection" (change)="onFilterChange()" [disabled]="!selectedClass">
            <option value="">All sections</option>
            @for (s of sectionsForClass(); track s) {
              <option [value]="s">{{ s }}</option>
            }
          </select>
        </div>
        <div class="col-12 col-md-3">
          <button class="btn btn-outline-primary w-100" (click)="load()" [disabled]="!selectedClass || loadingStudents()">
            <i class="bi bi-search me-1"></i> Load Students
          </button>
        </div>
      </div>
    </div>

    @if (loadingStudents()) {
      <div class="text-center py-4"><div class="spinner-border text-accent"></div></div>
    }

    @if (attendance()) {
      <div class="ev-card p-3">
        <!-- Header row -->
        <div class="d-flex align-items-center justify-content-between mb-3 flex-wrap gap-2">
          <div>
            <span class="fw-semibold">Class {{ selectedClass }}{{ selectedSection ? ' – ' + selectedSection : '' }}</span>
            <span class="ms-2 text-secondary-ev small">{{ attendance()!.totalStudents }} students</span>
            @if (attendance()!.alreadySaved) {
              <span class="badge bg-warning text-dark ms-2">Previously saved — editing</span>
            }
          </div>
          <div class="d-flex gap-2">
            <button class="btn btn-sm btn-outline-success" (click)="markAll(true)">All Present</button>
            <button class="btn btn-sm btn-outline-danger" (click)="markAll(false)">All Absent</button>
          </div>
        </div>

        <!-- Summary pills -->
        <div class="d-flex gap-3 mb-3">
          <span class="ev-pill ev-pill--success">Present: {{ presentCount() }}</span>
          <span class="ev-pill ev-pill--danger">Absent: {{ absentCount() }}</span>
        </div>

        <!-- Student list -->
        <div class="ev-attendance-list">
          @for (entry of entries(); track entry.studentId; let i = $index) {
            <div class="ev-att-row" [class.ev-att-row--present]="entry.isPresent" [class.ev-att-row--absent]="!entry.isPresent">
              <div class="ev-att-row__name">
                <span class="ev-att-row__num">{{ i + 1 }}</span>
                {{ entry.studentName }}
              </div>
              <div class="ev-att-row__toggle">
                <button class="ev-toggle-btn" [class.active]="entry.isPresent" (click)="toggle(entry, true)">
                  <i class="bi bi-check-lg"></i> Present
                </button>
                <button class="ev-toggle-btn ev-toggle-btn--absent" [class.active]="!entry.isPresent" (click)="toggle(entry, false)">
                  <i class="bi bi-x-lg"></i> Absent
                </button>
              </div>
              @if (!entry.isPresent) {
                <div class="ev-att-row__remarks">
                  <input type="text" class="form-control form-control-sm" placeholder="Remarks (optional)"
                    [(ngModel)]="entry.remarks" />
                </div>
              }
            </div>
          }
        </div>

        <div class="mt-4 d-flex justify-content-end">
          <button class="btn btn-primary px-4" (click)="submit()" [disabled]="saving()">
            @if (saving()) { <span class="spinner-border spinner-border-sm me-2"></span> }
            Save Attendance
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .ev-card { background: var(--ev-bg-card); border-radius: 14px; border: 1px solid var(--ev-border); }
    .ev-pill { border-radius: 20px; padding: .25rem .75rem; font-size: .82rem; font-weight: 600; }
    .ev-pill--success { background: #22c55e20; color: #22c55e; }
    .ev-pill--danger { background: #ef444420; color: #ef4444; }
    .ev-attendance-list { display: flex; flex-direction: column; gap: .5rem; }
    .ev-att-row { display: flex; align-items: center; gap: 1rem; padding: .75rem 1rem; border-radius: 10px; background: var(--ev-bg-elevated); border: 1px solid transparent; flex-wrap: wrap; }
    .ev-att-row--present { border-color: #22c55e33; }
    .ev-att-row--absent { border-color: #ef444433; }
    .ev-att-row__num { display: inline-block; width: 1.5rem; text-align: right; color: var(--ev-text-muted); font-size: .8rem; margin-right: .5rem; }
    .ev-att-row__name { flex: 1; min-width: 150px; font-weight: 500; font-size: .9rem; }
    .ev-att-row__toggle { display: flex; gap: .4rem; }
    .ev-att-row__remarks { flex: 1; min-width: 180px; }
    .ev-toggle-btn { border: 1px solid var(--ev-border); background: transparent; color: var(--ev-text-muted); border-radius: 8px; padding: .3rem .7rem; font-size: .82rem; cursor: pointer; transition: all .15s; }
    .ev-toggle-btn.active { background: #22c55e; border-color: #22c55e; color: #fff; }
    .ev-toggle-btn--absent.active { background: #ef4444; border-color: #ef4444; color: #fff; }
  `],
})
export class AttendanceComponent implements OnInit {
  private readonly service = inject(TeacherService);
  private readonly toast = inject(ToastService);

  readonly today = new Date().toISOString().split('T')[0];
  selectedDate = this.today;
  selectedClass = '';
  selectedSection = '';

  readonly classSections = signal<ClassSection[]>([]);
  readonly attendance = signal<AttendanceResult | null>(null);
  readonly entries = signal<{ studentId: string; studentName: string; isPresent: boolean; remarks?: string }[]>([]);
  readonly loadingStudents = signal(false);
  readonly saving = signal(false);

  readonly uniqueClasses = computed(() =>
    [...new Set(this.classSections().map((c) => c.class))].sort()
  );

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
    this.attendance.set(null);
    this.entries.set([]);
  }

  onFilterChange(): void {
    this.attendance.set(null);
    this.entries.set([]);
  }

  load(): void {
    if (!this.selectedClass) return;
    this.loadingStudents.set(true);
    this.service.getAttendance(this.selectedDate, this.selectedClass, this.selectedSection || undefined).subscribe({
      next: (result) => {
        this.attendance.set(result);
        this.entries.set(result.entries.map((e) => ({ ...e })));
        this.loadingStudents.set(false);
      },
      error: () => this.loadingStudents.set(false),
    });
  }

  toggle(entry: { studentId: string; studentName: string; isPresent: boolean; remarks?: string }, present: boolean): void {
    entry.isPresent = present;
    if (present) entry.remarks = undefined;
    this.entries.update((list) => [...list]);
  }

  markAll(present: boolean): void {
    this.entries.update((list) => list.map((e) => ({ ...e, isPresent: present, remarks: present ? undefined : e.remarks })));
  }

  readonly presentCount = computed(() => this.entries().filter((e) => e.isPresent).length);
  readonly absentCount = computed(() => this.entries().filter((e) => !e.isPresent).length);

  submit(): void {
    if (!this.attendance()) return;
    this.saving.set(true);
    this.service.markAttendance({
      date: new Date(this.selectedDate).toISOString(),
      class: this.selectedClass,
      section: this.selectedSection || undefined,
      entries: this.entries().map((e) => ({ studentId: e.studentId, isPresent: e.isPresent, remarks: e.remarks })),
    }).subscribe({
      next: (result) => {
        this.attendance.set(result);
        this.entries.set(result.entries.map((e) => ({ ...e })));
        this.saving.set(false);
        this.toast.success('Attendance saved', `${result.presentCount} present, ${result.absentCount} absent.`);
      },
      error: () => { this.saving.set(false); this.toast.error('Error', 'Failed to save attendance.'); },
    });
  }
}
