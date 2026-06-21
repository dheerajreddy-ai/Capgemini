import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

type Variant = 'success' | 'neutral' | 'warning' | 'danger' | 'info' | 'primary';

const MAP: Record<string, Variant> = {
  // Call status
  Completed: 'success', InProgress: 'info', Initiated: 'info', Ringing: 'info',
  Failed: 'danger', NoAnswer: 'neutral', Busy: 'warning',
  // Campaign status
  Running: 'info', Paused: 'warning', Draft: 'neutral',
  // Fees status
  Paid: 'success', Partial: 'warning', Unpaid: 'danger', Overdue: 'danger',
  // Complaint status
  New: 'danger', Read: 'info', Resolved: 'success', Closed: 'neutral',
  // Priority
  Low: 'neutral', Medium: 'info', High: 'warning', Urgent: 'danger',
};

@Component({
  selector: 'ev-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="ev-badge ev-badge--{{ variant() }}">{{ label() }}</span>`,
})
export class StatusBadgeComponent {
  readonly value = input.required<string>();
  readonly variant = computed<Variant>(() => MAP[this.value()] ?? 'neutral');
  readonly label = computed(() => this.value().replace(/([A-Z])/g, ' $1').trim());
}
