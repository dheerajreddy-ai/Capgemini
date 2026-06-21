import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { StudentsService, StudentQuery } from '../../core/services/students.service';
import { ToastService } from '../../core/services/toast.service';
import { ConfirmService } from '../../shared/components/confirm-dialog/confirm.service';
import { Student, PagedResult } from '../../core/models/models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { StudentFormComponent } from './student-form.component';
import { ImportStudentsComponent } from './import-students.component';
import { InstalmentPlanComponent } from '../instalments/instalment-plan.component';

@Component({
  selector: 'ev-students',
  standalone: true,
  imports: [
    RouterLink, FormsModule, StatusBadgeComponent, PaginationComponent, EmptyStateComponent,
    SkeletonComponent, InitialsPipe, RelativeTimePipe, StudentFormComponent, ImportStudentsComponent,
    InstalmentPlanComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './students.component.html',
})
export class StudentsComponent implements OnInit {
  private readonly service = inject(StudentsService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);
  private readonly search$ = new Subject<string>();

  readonly loading = signal(true);
  readonly result = signal<PagedResult<Student> | null>(null);
  readonly query = signal<StudentQuery>({ page: 1, pageSize: 20, search: '', class: '', feesStatus: '', sortBy: 'fullName' });

  readonly editing = signal<Student | null>(null);
  readonly showForm = signal(false);
  readonly showImport = signal(false);
  readonly instalmentStudent = signal<Student | null>(null);

  readonly feesOptions = ['', 'Paid', 'Partial', 'Unpaid', 'Overdue'];

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => {
      this.query.update((q) => ({ ...q, search: term, page: 1 }));
      this.load();
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.list(this.query()).subscribe({
      next: (r) => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  onSearch(term: string): void { this.search$.next(term); }

  setFilter(key: 'class' | 'feesStatus', value: string): void {
    this.query.update((q) => ({ ...q, [key]: value, page: 1 }));
    this.load();
  }

  setPage(page: number): void {
    this.query.update((q) => ({ ...q, page }));
    this.load();
  }

  openCreate(): void { this.editing.set(null); this.showForm.set(true); }
  openEdit(s: Student): void { this.editing.set(s); this.showForm.set(true); }
  openInstalments(s: Student): void { this.instalmentStudent.set(s); }

  onSaved(): void { this.showForm.set(false); this.toast.success('Saved', 'Student details updated.'); this.load(); }
  onImported(): void { this.showImport.set(false); this.load(); }

  async remove(s: Student): Promise<void> {
    const ok = await this.confirm.ask({
      title: 'Remove student?',
      message: `${s.fullName} will be archived. You can restore them later.`,
      danger: true, confirmText: 'Remove', icon: 'bi-trash',
    });
    if (!ok) return;
    this.service.remove(s.id).subscribe(() => { this.toast.success('Removed', `${s.fullName} archived.`); this.load(); });
  }

  exportExcel(): void {
    this.service.export().subscribe((blob) => this.downloadBlob(blob, 'students.xlsx'));
  }

  feesTone(status: string): string {
    return status === 'Paid' ? 'text-success' : status === 'Overdue' || status === 'Unpaid' ? 'text-danger' : 'text-warning';
  }

  private downloadBlob(blob: Blob, name: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = name; a.click();
    URL.revokeObjectURL(url);
  }
}
