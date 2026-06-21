import { Component, ChangeDetectionStrategy, inject, signal, input, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { StudentsService } from '../../core/services/students.service';
import { Student } from '../../core/models/models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';

@Component({
  selector: 'ev-student-detail',
  standalone: true,
  imports: [RouterLink, DatePipe, StatusBadgeComponent, SkeletonComponent, InitialsPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a routerLink="/students" class="ev-link small fw-semibold d-inline-block mb-3"><i class="bi bi-arrow-left me-1"></i>Back to students</a>

    @if (loading()) {
      <div class="ev-card ev-card-pad"><ev-skeleton [rows]="6" [height]="26" /></div>
    } @else if (student(); as s) {
      <!-- Hero -->
      <div class="ev-card ev-card-pad mb-3">
        <div class="d-flex align-items-center gap-3 flex-wrap">
          <div class="ev-avatar ev-avatar--lg">{{ s.fullName | evInitials }}</div>
          <div class="flex-grow-1">
            <h1 class="ev-page-title mb-1">{{ s.fullName }}</h1>
            <div class="d-flex gap-2 flex-wrap text-secondary-ev small">
              <span><i class="bi bi-mortarboard me-1"></i>{{ s.class }} · {{ s.section }}</span>
              <span><i class="bi bi-person me-1"></i>{{ s.parentName }}</span>
              <span><i class="bi bi-telephone me-1"></i>{{ s.parentPhone }}</span>
              <span><i class="bi bi-upc-scan me-1"></i>{{ s.studentCode }}</span>
            </div>
          </div>
          <button class="btn btn-primary"><i class="bi bi-telephone-outbound me-2"></i>Call Now</button>
        </div>
      </div>

      <!-- Tabs -->
      <ul class="nav ev-tabs mb-3">
        @for (t of tabs; track t.id) {
          <li><button class="ev-tab" [class.is-active]="tab() === t.id" (click)="tab.set(t.id)">{{ t.label }}</button></li>
        }
      </ul>

      @switch (tab()) {
        @case ('marks') {
          <div class="row g-3">
            @for (m of marks(s); track m.label) {
              <div class="col-6 col-md-4 col-lg-3">
                <div class="ev-card ev-card-pad text-center h-100">
                  <div class="ev-subject-score" [class.is-weak]="m.value !== null && m.value < 60">{{ m.value ?? '—' }}<small>%</small></div>
                  <div class="text-secondary-ev small mt-1">{{ m.label }}</div>
                  @if (m.value !== null) {
                    <div class="ev-progress mt-2"><div class="ev-progress__bar" [style.width.%]="m.value" [class.bar-weak]="m.value < 60"></div></div>
                  }
                </div>
              </div>
            }
          </div>
        }
        @case ('fees') {
          <div class="ev-card ev-card-pad">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <div class="text-secondary-ev small">Outstanding fees</div>
                <div class="ev-stat__value">₹{{ s.feesDue.toLocaleString('en-IN') }}</div>
              </div>
              <ev-status-badge [value]="s.feesStatus" />
            </div>
            @if (s.feesDueDate) { <hr class="ev-soft-divider" /><div class="small text-secondary-ev"><i class="bi bi-calendar me-1"></i>Due {{ s.feesDueDate | date }}</div> }
          </div>
        }
        @default {
          <div class="ev-card">
            <div class="ev-card-pad">
              <div class="row g-3">
                <div class="col-md-6"><div class="ev-info-row"><span>Attendance</span><b>{{ s.attendance ?? '—' }}%</b></div></div>
                <div class="col-md-6"><div class="ev-info-row"><span>WhatsApp</span><b>{{ s.parentWhatsApp ?? '—' }}</b></div></div>
                <div class="col-md-6"><div class="ev-info-row"><span>Alternate phone</span><b>{{ s.parentPhone2 ?? '—' }}</b></div></div>
                <div class="col-md-6"><div class="ev-info-row"><span>Fees status</span><b>{{ s.feesStatus }}</b></div></div>
                <div class="col-12"><div class="ev-info-row"><span>Notes</span><b>{{ s.notes || '—' }}</b></div></div>
              </div>
            </div>
          </div>
        }
      }
    }
  `,
  styles: [`
    .ev-tabs { gap: .35rem; border-bottom: 1px solid var(--ev-border); padding-bottom: 0; }
    .ev-tab { background: none; border: none; padding: .65rem 1rem; font-weight: 600; color: var(--ev-text-secondary); border-bottom: 2px solid transparent; margin-bottom: -1px; }
    .ev-tab.is-active { color: var(--ev-primary); border-bottom-color: var(--ev-primary); }
    .ev-tab:hover { color: var(--ev-primary); }
    .ev-subject-score { font-family: "Sora",sans-serif; font-size: 2rem; font-weight: 800; color: var(--ev-primary); }
    .ev-subject-score small { font-size: 1rem; color: var(--ev-text-muted); }
    .ev-subject-score.is-weak { color: var(--ev-danger); }
    .bar-weak { background: var(--ev-grad-danger) !important; }
    .ev-info-row { display: flex; justify-content: space-between; padding: .6rem 0; border-bottom: 1px solid var(--ev-border); }
    .ev-info-row span { color: var(--ev-text-secondary); font-size: .88rem; }
  `],
})
export class StudentDetailComponent implements OnInit {
  private readonly service = inject(StudentsService);

  readonly id = input.required<string>();
  readonly loading = signal(true);
  readonly student = signal<Student | null>(null);
  readonly tab = signal<'overview' | 'marks' | 'fees'>('overview');

  readonly tabs = [
    { id: 'overview', label: 'Overview' },
    { id: 'marks', label: 'Marks' },
    { id: 'fees', label: 'Fees' },
  ] as const;

  ngOnInit(): void {
    this.service.get(this.id()).subscribe({
      next: (s) => { this.student.set(s); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  marks(s: Student): { label: string; value: number | null }[] {
    return [
      { label: 'Maths', value: s.mathsMarks ?? null },
      { label: 'Science', value: s.scienceMarks ?? null },
      { label: 'English', value: s.englishMarks ?? null },
      { label: 'Telugu', value: s.teluguMarks ?? null },
      { label: 'Hindi', value: s.hindiMarks ?? null },
      { label: 'Social', value: s.socialMarks ?? null },
    ];
  }
}
