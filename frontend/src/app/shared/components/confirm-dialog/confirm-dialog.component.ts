import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { ConfirmService } from './confirm.service';

@Component({
  selector: 'ev-confirm-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (confirm.state(); as s) {
      <div class="ev-backdrop" (click)="confirm.resolve(false)"></div>
      <div class="ev-confirm ev-fade-up" role="dialog" aria-modal="true">
        <div class="ev-confirm__icon" [class.is-danger]="s.danger">
          <i class="bi {{ s.icon ?? (s.danger ? 'bi-exclamation-triangle' : 'bi-question-circle') }}"></i>
        </div>
        <h5 class="mb-1">{{ s.title }}</h5>
        <p class="text-secondary-ev mb-4">{{ s.message }}</p>
        <div class="d-flex gap-2 justify-content-center">
          <button class="btn btn-soft px-4" (click)="confirm.resolve(false)">{{ s.cancelText }}</button>
          <button class="btn px-4" [class.btn-danger]="s.danger" [class.btn-primary]="!s.danger" (click)="confirm.resolve(true)">
            {{ s.confirmText }}
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .ev-backdrop { position: fixed; inset: 0; background: rgba(15,23,42,.5); backdrop-filter: blur(3px); z-index: 1080; }
    .ev-confirm {
      position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%);
      z-index: 1085; background: #fff; border-radius: 22px; padding: 2rem;
      width: min(420px, 92vw); text-align: center; box-shadow: var(--ev-shadow-xl);
    }
    .ev-confirm__icon {
      width: 64px; height: 64px; margin: 0 auto 1.1rem; border-radius: 20px;
      display: grid; place-items: center; font-size: 1.7rem;
      background: var(--ev-primary-soft); color: var(--ev-primary);
    }
    .ev-confirm__icon.is-danger { background: var(--ev-danger-soft); color: var(--ev-danger); }
    .btn-danger { background: var(--ev-grad-danger); border: none; box-shadow: 0 10px 24px rgba(239,68,68,.3); }
  `],
})
export class ConfirmDialogComponent {
  readonly confirm = inject(ConfirmService);
}
