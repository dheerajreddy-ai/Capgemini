import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { CallsService, CallQuery } from '../../core/services/calls.service';
import { Call, PagedResult } from '../../core/models/models';
import { SentimentBadgeComponent } from '../../shared/components/sentiment-badge/sentiment-badge.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';
import { DurationPipe } from '../../shared/pipes/duration.pipe';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';
import { CallDetailPanelComponent } from './call-detail-panel.component';

@Component({
  selector: 'ev-calls',
  standalone: true,
  imports: [
    FormsModule, SentimentBadgeComponent, StatusBadgeComponent, PaginationComponent,
    EmptyStateComponent, SkeletonComponent, InitialsPipe, DurationPipe, RelativeTimePipe,
    CallDetailPanelComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './calls.component.html',
})
export class CallsComponent implements OnInit {
  private readonly service = inject(CallsService);
  private readonly search$ = new Subject<string>();

  readonly loading = signal(true);
  readonly result = signal<PagedResult<Call> | null>(null);
  readonly query = signal<CallQuery>({ page: 1, pageSize: 20 });
  readonly selected = signal<Call | null>(null);

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((s) => {
      this.query.update((q) => ({ ...q, search: s, page: 1 }));
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

  onSearch(s: string): void { this.search$.next(s); }

  setFilter(key: keyof CallQuery, value: string | boolean): void {
    this.query.update((q) => ({ ...q, [key]: value === '' ? undefined : value, page: 1 }));
    this.load();
  }

  setPage(page: number): void { this.query.update((q) => ({ ...q, page })); this.load(); }

  open(call: Call): void {
    this.service.get(call.id).subscribe((full) => this.selected.set(full));
  }
}
