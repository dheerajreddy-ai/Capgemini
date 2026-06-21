import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed } from '@angular/core';
import { AdminService, AdminSchool } from '../../core/services/admin.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';

@Component({
  selector: 'ev-admin-schools',
  standalone: true,
  imports: [StatusBadgeComponent, EmptyStateComponent, SkeletonComponent, StatCardComponent, InitialsPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-page-head">
      <div>
        <h1 class="ev-page-title">Schools</h1>
        <p class="ev-page-sub">Platform-wide tenant management</p>
      </div>
      <button class="btn btn-primary"><i class="bi bi-plus-lg me-2"></i>Add School</button>
    </div>

    <div class="row g-3 mb-3">
      <div class="col-6 col-xl-3"><ev-stat-card icon="bi-buildings" tone="primary" [value]="schools().length" label="Total Schools" /></div>
      <div class="col-6 col-xl-3"><ev-stat-card icon="bi-telephone" tone="info" [value]="totalCalls()" label="Total Calls" /></div>
      <div class="col-6 col-xl-3"><ev-stat-card icon="bi-people" tone="success" [value]="totalStudents()" label="Total Students" /></div>
      <div class="col-6 col-xl-3"><ev-stat-card icon="bi-cash-stack" tone="warning" [value]="'₹' + totalRevenue().toLocaleString('en-IN')" label="MRR" /></div>
    </div>

    <div class="ev-card">
      <div class="ev-card-head"><h3 class="ev-card-title">All Schools</h3></div>
      @if (loading()) {
        <div class="ev-card-pad"><ev-skeleton [rows]="6" [height]="26" /></div>
      } @else if (schools().length === 0) {
        <ev-empty-state icon="bi-buildings" title="No schools yet" message="Onboard your first school tenant to get started." />
      } @else {
        <div class="ev-table-wrap">
          <table class="ev-table">
            <thead><tr><th>School</th><th>Plan</th><th>Students</th><th>Calls</th><th>Revenue</th><th>Status</th></tr></thead>
            <tbody>
              @for (s of schools(); track s.id) {
                <tr>
                  <td>
                    <div class="d-flex align-items-center gap-2">
                      @if (s.logoUrl) { <img [src]="s.logoUrl" class="ev-avatar" alt="" /> } @else { <div class="ev-avatar">{{ s.name | evInitials }}</div> }
                      <div><div class="ev-cell-strong">{{ s.name }}</div><div class="ev-cell-sub">{{ s.subDomain }}.eduvoice.in</div></div>
                    </div>
                  </td>
                  <td><span class="ev-badge ev-badge--primary">{{ s.planType }}</span></td>
                  <td class="ev-cell-sub">{{ s.totalStudents }}</td>
                  <td class="ev-cell-sub">{{ s.totalCalls }}</td>
                  <td class="ev-cell-strong">₹{{ s.monthlyRevenue.toLocaleString('en-IN') }}</td>
                  <td><ev-status-badge [value]="s.status" /></td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `,
})
export class AdminSchoolsComponent implements OnInit {
  private readonly service = inject(AdminService);

  readonly loading = signal(true);
  readonly schools = signal<AdminSchool[]>([]);

  readonly totalCalls = computed(() => this.schools().reduce((a, s) => a + s.totalCalls, 0));
  readonly totalStudents = computed(() => this.schools().reduce((a, s) => a + s.totalStudents, 0));
  readonly totalRevenue = computed(() => this.schools().reduce((a, s) => a + s.monthlyRevenue, 0));

  ngOnInit(): void {
    this.service.listSchools().subscribe({
      next: (s) => { this.schools.set(s); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
