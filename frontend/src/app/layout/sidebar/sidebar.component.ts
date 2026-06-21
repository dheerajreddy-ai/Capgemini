import { Component, ChangeDetectionStrategy, inject, input, output, computed } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  badge?: () => number | null;
}

@Component({
  selector: 'ev-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, InitialsPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <aside class="ev-sidebar" [class.is-open]="open()">
      <!-- Brand -->
      <div class="ev-sidebar__brand">
        @if (school()?.logoUrl) {
          <img [src]="school()!.logoUrl" alt="logo" class="ev-sidebar__logo" />
        } @else {
          <div class="ev-sidebar__logo ev-sidebar__logo--fallback">
            {{ (school()?.name ?? 'EduVoice') | evInitials }}
          </div>
        }
        <div class="ev-sidebar__brandtext">
          <div class="ev-sidebar__name">{{ school()?.name ?? 'EduVoice' }}</div>
          <div class="ev-sidebar__tag">
            <span class="ev-dot ev-dot--live"></span> AI Voice Agent
          </div>
        </div>
        <button class="ev-sidebar__close d-lg-none" (click)="close.emit()" aria-label="Close menu">
          <i class="bi bi-x-lg"></i>
        </button>
      </div>

      <!-- Nav -->
      <nav class="ev-sidebar__nav">
        <div class="ev-sidebar__section">Overview</div>
        @for (item of mainNav; track item.route) {
          <a class="ev-nav-link" [routerLink]="item.route" routerLinkActive="is-active" (click)="close.emit()">
            <i class="bi {{ item.icon }}"></i>
            <span>{{ item.label }}</span>
            @if (item.badge && item.badge()) {
              <span class="ev-nav-badge">{{ item.badge() }}</span>
            }
          </a>
        }

        @if (isSuperAdmin()) {
          <div class="ev-sidebar__section mt-3">Platform Admin</div>
          <a class="ev-nav-link" routerLink="/admin/schools" routerLinkActive="is-active" (click)="close.emit()">
            <i class="bi bi-buildings"></i><span>Schools</span>
          </a>
        }

        <div class="ev-sidebar__section mt-3">Account</div>
        <a class="ev-nav-link" routerLink="/settings" routerLinkActive="is-active" (click)="close.emit()">
          <i class="bi bi-gear"></i><span>Settings</span>
        </a>
      </nav>

      <!-- Upgrade / plan card -->
      <div class="ev-sidebar__foot">
        <div class="ev-plan-card">
          <div class="ev-plan-card__shine"></div>
          <div class="ev-plan-card__badge">{{ school()?.planType ?? 'Starter' }} Plan</div>
          <div class="ev-plan-card__title">Unlock more calls</div>
          <p class="ev-plan-card__sub">Scale parent outreach with higher limits and analytics.</p>
          <a routerLink="/settings" class="btn btn-light w-100 btn-sm fw-bold">Upgrade</a>
        </div>
      </div>
    </aside>
  `,
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  private readonly auth = inject(AuthService);

  readonly open = input(false);
  readonly close = output<void>();

  readonly school = this.auth.school;
  readonly isSuperAdmin = this.auth.isSuperAdmin;

  readonly mainNav: NavItem[] = [
    { label: 'Dashboard', icon: 'bi-grid-1x2', route: '/dashboard' },
    { label: 'Students', icon: 'bi-mortarboard', route: '/students' },
    { label: 'Campaigns', icon: 'bi-megaphone', route: '/campaigns' },
    { label: 'Call Logs', icon: 'bi-telephone', route: '/calls' },
    { label: 'Complaints', icon: 'bi-chat-square-dots', route: '/complaints' },
    { label: 'Broadcasts', icon: 'bi-broadcast', route: '/broadcasts' },
    { label: 'Analytics', icon: 'bi-graph-up-arrow', route: '/analytics' },
    { label: 'Exam Schedule', icon: 'bi-calendar-check', route: '/exam-schedule' },
    { label: 'Homework', icon: 'bi-journal-text', route: '/homework' },
    { label: 'Dropout Risk', icon: 'bi-exclamation-triangle', route: '/dropout-risk' },
    { label: 'Fee Collection', icon: 'bi-cash-stack', route: '/fee-collection' },
    { label: 'Parent Engagement', icon: 'bi-heart', route: '/parent-engagement' },
  ];
}
