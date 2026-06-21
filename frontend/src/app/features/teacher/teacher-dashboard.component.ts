import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TeacherService } from './teacher.service';
import { AuthService } from '../../core/auth/auth.service';
import { TeacherDashboard, ClassSection } from '../../core/models/models';

@Component({
  selector: 'ev-teacher-dashboard',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-header mb-4">
      <div>
        <h1 class="ev-page-title">Teacher Portal</h1>
        <p class="ev-page-sub text-secondary-ev">Welcome, {{ userName() }}</p>
      </div>
    </div>

    @if (loading()) {
      <div class="text-center py-5"><div class="spinner-border text-accent"></div></div>
    } @else if (data()) {
      <!-- Stats row -->
      <div class="row g-3 mb-4">
        <div class="col-6 col-lg-3">
          <div class="ev-stat-card">
            <div class="ev-stat-card__icon"><i class="bi bi-people-fill"></i></div>
            <div class="ev-stat-card__val">{{ data()!.totalStudents }}</div>
            <div class="ev-stat-card__label">Total Students</div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="ev-stat-card" [class.ev-stat-card--success]="data()!.attendanceMarkedToday">
            <div class="ev-stat-card__icon"><i class="bi bi-check-circle-fill"></i></div>
            <div class="ev-stat-card__val">{{ data()!.todayPresent }}</div>
            <div class="ev-stat-card__label">Present Today</div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="ev-stat-card" [class.ev-stat-card--warn]="data()!.todayAbsent > 0">
            <div class="ev-stat-card__icon"><i class="bi bi-x-circle-fill"></i></div>
            <div class="ev-stat-card__val">{{ data()!.todayAbsent }}</div>
            <div class="ev-stat-card__label">Absent Today</div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="ev-stat-card">
            <div class="ev-stat-card__icon"><i class="bi bi-journal-text"></i></div>
            <div class="ev-stat-card__val">{{ data()!.activeHomeworkCount }}</div>
            <div class="ev-stat-card__label">Active Homework</div>
          </div>
        </div>
      </div>

      <!-- Attendance banner -->
      @if (!data()!.attendanceMarkedToday) {
        <div class="ev-alert ev-alert--warn d-flex align-items-center justify-content-between mb-4">
          <span><i class="bi bi-exclamation-triangle me-2"></i>Attendance not yet marked for today.</span>
          <a routerLink="/teacher/attendance" class="btn btn-sm btn-warning fw-bold">Mark Now</a>
        </div>
      } @else {
        <div class="ev-alert ev-alert--success d-flex align-items-center mb-4">
          <i class="bi bi-check-circle me-2"></i>Attendance marked for today. {{ data()!.todayPresent }} present, {{ data()!.todayAbsent }} absent.
        </div>
      }

      <!-- Quick actions -->
      <div class="row g-3 mb-4">
        <div class="col-12">
          <div class="ev-card p-3">
            <h6 class="ev-card-title mb-3">Quick Actions</h6>
            <div class="d-flex flex-wrap gap-2">
              <a routerLink="/teacher/attendance" class="btn btn-primary">
                <i class="bi bi-calendar-check me-2"></i>Mark Attendance
              </a>
              <a routerLink="/teacher/marks" class="btn btn-outline-primary">
                <i class="bi bi-pencil-square me-2"></i>Upload Marks
              </a>
              <a routerLink="/teacher/homework" class="btn btn-outline-primary">
                <i class="bi bi-journal-plus me-2"></i>Assign Homework
              </a>
            </div>
          </div>
        </div>
      </div>

      <!-- Class sections -->
      @if (data()!.classSections.length > 0) {
        <div class="ev-card p-3">
          <h6 class="ev-card-title mb-3">Your Classes ({{ data()!.classSections.length }})</h6>
          <div class="row g-2">
            @for (cs of data()!.classSections; track cs.class + cs.section) {
              <div class="col-6 col-md-4 col-lg-3">
                <div class="ev-class-chip">
                  <span class="ev-class-chip__name">Class {{ cs.class }}{{ cs.section ? ' – ' + cs.section : '' }}</span>
                  <span class="ev-class-chip__count">{{ cs.studentCount }} students</span>
                </div>
              </div>
            }
          </div>
        </div>
      }
    }
  `,
  styles: [`
    .ev-stat-card { background: var(--ev-bg-card); border-radius: 14px; padding: 1.25rem; border: 1px solid var(--ev-border); }
    .ev-stat-card--success { border-color: #22c55e44; }
    .ev-stat-card--warn { border-color: #f59e0b44; }
    .ev-stat-card__icon { font-size: 1.4rem; color: var(--ev-accent); margin-bottom: .5rem; }
    .ev-stat-card__val { font-size: 2rem; font-weight: 700; color: var(--ev-text); line-height: 1; }
    .ev-stat-card__label { font-size: .78rem; color: var(--ev-text-muted); margin-top: .25rem; }
    .ev-alert { border-radius: 10px; padding: .85rem 1rem; font-size: .9rem; }
    .ev-alert--warn { background: #f59e0b18; border: 1px solid #f59e0b44; color: #f59e0b; }
    .ev-alert--success { background: #22c55e18; border: 1px solid #22c55e44; color: #22c55e; }
    .ev-class-chip { background: var(--ev-bg-elevated); border-radius: 8px; padding: .6rem .8rem; border: 1px solid var(--ev-border); }
    .ev-class-chip__name { display: block; font-weight: 600; font-size: .88rem; color: var(--ev-text); }
    .ev-class-chip__count { font-size: .75rem; color: var(--ev-text-muted); }
    .ev-card { background: var(--ev-bg-card); border-radius: 14px; border: 1px solid var(--ev-border); }
    .ev-card-title { font-weight: 600; color: var(--ev-text); font-size: .95rem; }
  `],
})
export class TeacherDashboardComponent implements OnInit {
  private readonly service = inject(TeacherService);
  private readonly auth = inject(AuthService);

  readonly loading = signal(true);
  readonly data = signal<TeacherDashboard | null>(null);

  readonly userName = () => {
    const u = this.auth.user();
    return u ? `${u.firstName} ${u.lastName}`.trim() : 'Teacher';
  };

  ngOnInit(): void {
    this.service.getDashboard().subscribe({
      next: (d) => { this.data.set(d); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
