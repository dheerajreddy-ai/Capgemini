import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { HeaderComponent } from '../header/header.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'ev-main-layout',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, HeaderComponent, ConfirmDialogComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-shell">
      <ev-sidebar [open]="menuOpen()" (close)="menuOpen.set(false)" />

      @if (menuOpen()) {
        <div class="ev-shell__scrim d-lg-none" (click)="menuOpen.set(false)"></div>
      }

      <div class="ev-shell__main">
        <ev-header (toggleMenu)="menuOpen.set(!menuOpen())" />
        <main class="ev-shell__content">
          <div class="ev-fade-up">
            <router-outlet />
          </div>
        </main>
      </div>
    </div>

    <ev-confirm-dialog />
  `,
  styles: [`
    .ev-shell { min-height: 100vh; background: var(--ev-bg); }
    .ev-shell::before {
      content: ""; position: fixed; inset: 0 0 auto 0; height: 360px;
      background: var(--ev-grad-aurora); pointer-events: none; z-index: 0;
    }
    .ev-shell__main {
      margin-left: var(--ev-sidebar-w);
      position: relative; z-index: 1;
      transition: margin-left .3s ease;
    }
    .ev-shell__content { padding: 1.75rem; max-width: 1480px; margin: 0 auto; }
    .ev-shell__scrim {
      position: fixed; inset: 0; z-index: 1040;
      background: rgba(15, 23, 42, .45); backdrop-filter: blur(2px);
    }
    @media (max-width: 991.98px) {
      .ev-shell__main { margin-left: 0; }
      .ev-shell__content { padding: 1.1rem; }
    }
  `],
})
export class MainLayoutComponent {
  readonly menuOpen = signal(false);
}
