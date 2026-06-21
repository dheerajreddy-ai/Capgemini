import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';

@Component({
  selector: 'ev-pagination',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (totalPages() > 1) {
      <div class="d-flex align-items-center justify-content-between flex-wrap gap-2 pt-3">
        <span class="text-secondary-ev small">
          Showing {{ from() }}–{{ to() }} of {{ total() }}
        </span>
        <div class="d-flex align-items-center gap-1">
          <button class="btn btn-soft btn-sm btn-icon" [disabled]="page() === 1" (click)="go(page() - 1)" aria-label="Previous">
            <i class="bi bi-chevron-left"></i>
          </button>
          @for (p of pages(); track p) {
            @if (p === -1) {
              <span class="px-2 text-muted-ev">…</span>
            } @else {
              <button class="btn btn-sm ev-page" [class.ev-page--active]="p === page()" (click)="go(p)">{{ p }}</button>
            }
          }
          <button class="btn btn-soft btn-sm btn-icon" [disabled]="page() === totalPages()" (click)="go(page() + 1)" aria-label="Next">
            <i class="bi bi-chevron-right"></i>
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .ev-page { min-width: 38px; border-radius: 10px; border: 1px solid var(--ev-border-strong); background: #fff; font-weight: 600; color: var(--ev-text-secondary); }
    .ev-page:hover { background: var(--ev-primary-soft); color: var(--ev-primary); }
    .ev-page--active { background: var(--ev-grad-primary); color: #fff; border-color: transparent; box-shadow: var(--ev-shadow-primary); }
    .ev-page--active:hover { color: #fff; }
  `],
})
export class PaginationComponent {
  readonly page = input.required<number>();
  readonly pageSize = input(20);
  readonly total = input.required<number>();
  readonly pageChange = output<number>();

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.total() / this.pageSize())));
  readonly from = computed(() => (this.total() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1));
  readonly to = computed(() => Math.min(this.page() * this.pageSize(), this.total()));

  readonly pages = computed<number[]>(() => {
    const tp = this.totalPages();
    const cur = this.page();
    if (tp <= 7) return Array.from({ length: tp }, (_, i) => i + 1);
    const out: number[] = [1];
    if (cur > 3) out.push(-1);
    for (let p = Math.max(2, cur - 1); p <= Math.min(tp - 1, cur + 1); p++) out.push(p);
    if (cur < tp - 2) out.push(-1);
    out.push(tp);
    return out;
  });

  go(p: number): void {
    if (p >= 1 && p <= this.totalPages() && p !== this.page()) this.pageChange.emit(p);
  }
}
