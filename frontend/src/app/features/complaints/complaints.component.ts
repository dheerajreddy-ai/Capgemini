import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ComplaintsService, ComplaintQuery } from '../../core/services/complaints.service';
import { ToastService } from '../../core/services/toast.service';
import { Complaint, PagedResult } from '../../core/models/models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';
import { ComplaintDetailComponent } from './complaint-detail.component';

@Component({
  selector: 'ev-complaints',
  standalone: true,
  imports: [
    FormsModule, StatusBadgeComponent, PaginationComponent, EmptyStateComponent,
    SkeletonComponent, RelativeTimePipe, InitialsPipe, ComplaintDetailComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './complaints.component.html',
})
export class ComplaintsComponent implements OnInit {
  private readonly service = inject(ComplaintsService);
  private readonly toast = inject(ToastService);

  readonly loading = signal(true);
  readonly result = signal<PagedResult<Complaint> | null>(null);
  readonly query = signal<ComplaintQuery>({ page: 1, pageSize: 20 });
  readonly selected = signal<Complaint | null>(null);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.list(this.query()).subscribe({
      next: (r) => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  setFilter(key: keyof ComplaintQuery, value: string): void {
    this.query.update((q) => ({ ...q, [key]: value === '' ? undefined : value, page: 1 }));
    this.load();
  }

  setPage(page: number): void { this.query.update((q) => ({ ...q, page })); this.load(); }

  onUpdated(): void { this.selected.set(null); this.toast.success('Updated', 'Complaint updated.'); this.load(); }

  exportExcel(): void {
    this.service.export().subscribe((blob) => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url; a.download = 'complaints.xlsx'; a.click();
      URL.revokeObjectURL(url);
    });
  }

  countBy(status: string): number {
    return this.result()?.items.filter((c) => c.status === status).length ?? 0;
  }

  slaHoursLeft(c: Complaint): { h: number } | null {
    if (!c.slaDeadline || c.status === 'Resolved' || c.status === 'Closed') return null;
    return { h: Math.round((new Date(c.slaDeadline).getTime() - Date.now()) / 3_600_000) };
  }

  slaBadgeClass(h: number): string {
    if (h < 0) return 'ev-sla-badge ev-sla-badge--breach';
    if (h <= 4) return 'ev-sla-badge ev-sla-badge--critical';
    if (h <= 12) return 'ev-sla-badge ev-sla-badge--warning';
    return 'ev-sla-badge ev-sla-badge--ok';
  }
}
