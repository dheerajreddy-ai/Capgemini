import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'ev-loading-bar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading.active()) {
      <div class="ev-loadbar" role="progressbar" aria-label="Loading">
        <div class="ev-loadbar__inner"></div>
      </div>
    }
  `,
  styles: [`
    .ev-loadbar {
      position: fixed; top: 0; left: 0; right: 0; height: 3px; z-index: 2000;
      background: transparent; overflow: hidden;
    }
    .ev-loadbar__inner {
      height: 100%; width: 40%;
      background: var(--ev-grad-primary);
      border-radius: 0 4px 4px 0;
      animation: evSlide 1.1s ease-in-out infinite;
      box-shadow: 0 0 12px rgba(var(--ev-primary-rgb), .6);
    }
    @keyframes evSlide {
      0%   { transform: translateX(-100%); width: 40%; }
      50%  { width: 60%; }
      100% { transform: translateX(260%); width: 30%; }
    }
  `],
})
export class LoadingBarComponent {
  readonly loading = inject(LoadingService);
}
