import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { ToastService, ToastKind } from '../../../core/services/toast.service';

@Component({
  selector: 'ev-toast-host',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-toast-host">
      @for (t of toast.toasts(); track t.id) {
        <div class="ev-toast ev-toast--{{ t.kind }} ev-fade-up" role="alert">
          <div class="ev-toast__icon"><i class="bi {{ icon(t.kind) }}"></i></div>
          <div class="ev-toast__body">
            <div class="ev-toast__title">{{ t.title }}</div>
            @if (t.message) { <div class="ev-toast__msg">{{ t.message }}</div> }
          </div>
          <button class="ev-toast__close" (click)="toast.dismiss(t.id)" aria-label="Dismiss">
            <i class="bi bi-x"></i>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .ev-toast {
      display: flex; align-items: flex-start; gap: .8rem;
      background: #fff; border: 1px solid var(--ev-border);
      border-left: 4px solid var(--ev-primary);
      border-radius: 14px; padding: .9rem 1rem;
      box-shadow: var(--ev-shadow-lg); min-width: 320px;
    }
    .ev-toast--success { border-left-color: var(--ev-success); }
    .ev-toast--error   { border-left-color: var(--ev-danger); }
    .ev-toast--warning { border-left-color: var(--ev-warning); }
    .ev-toast--info    { border-left-color: var(--ev-info); }
    .ev-toast__icon { width: 34px; height: 34px; border-radius: 10px; display: grid; place-items: center; font-size: 1.05rem; flex-shrink: 0; }
    .ev-toast--success .ev-toast__icon { background: var(--ev-success-soft); color: #047857; }
    .ev-toast--error .ev-toast__icon   { background: var(--ev-danger-soft); color: #B91C1C; }
    .ev-toast--warning .ev-toast__icon { background: var(--ev-warning-soft); color: #B45309; }
    .ev-toast--info .ev-toast__icon    { background: var(--ev-info-soft); color: #4338CA; }
    .ev-toast__body { flex: 1; min-width: 0; }
    .ev-toast__title { font-weight: 700; font-size: .9rem; }
    .ev-toast__msg { font-size: .82rem; color: var(--ev-text-secondary); margin-top: .15rem; }
    .ev-toast__close { background: none; border: none; color: var(--ev-text-muted); font-size: 1.1rem; line-height: 1; cursor: pointer; padding: 0; }
    .ev-toast__close:hover { color: var(--ev-text); }
  `],
})
export class ToastHostComponent {
  readonly toast = inject(ToastService);

  icon(kind: ToastKind): string {
    return {
      success: 'bi-check-circle-fill',
      error: 'bi-exclamation-octagon-fill',
      warning: 'bi-exclamation-triangle-fill',
      info: 'bi-info-circle-fill',
    }[kind];
  }
}
