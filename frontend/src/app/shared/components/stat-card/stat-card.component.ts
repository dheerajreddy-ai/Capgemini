import { Component, ChangeDetectionStrategy, input } from '@angular/core';

@Component({
  selector: 'ev-stat-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-card ev-card--hover ev-stat h-100">
      <div class="d-flex align-items-start justify-content-between">
        <div class="ev-stat__icon ev-stat__icon--{{ tone() }}">
          <i class="bi {{ icon() }}"></i>
        </div>
        @if (trend() !== null && trend() !== undefined) {
          <span class="ev-stat__trend" [class.ev-trend-up]="trend()! >= 0" [class.ev-trend-down]="trend()! < 0">
            <i class="bi" [class.bi-arrow-up-right]="trend()! >= 0" [class.bi-arrow-down-right]="trend()! < 0"></i>
            {{ absTrend() }}%
          </span>
        }
      </div>
      <div class="ev-stat__value">{{ value() }}</div>
      <div class="ev-stat__label">{{ label() }}</div>
      @if (hint()) { <div class="ev-stat__label mt-1" style="font-size:.78rem">{{ hint() }}</div> }
    </div>
  `,
})
export class StatCardComponent {
  readonly icon = input.required<string>();
  readonly value = input.required<string | number>();
  readonly label = input.required<string>();
  readonly tone = input<'primary' | 'success' | 'warning' | 'danger' | 'info'>('primary');
  readonly trend = input<number | null>(null);
  readonly hint = input<string | null>(null);

  absTrend(): number { return Math.abs(this.trend() ?? 0); }
}
