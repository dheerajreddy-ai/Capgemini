import { Component, ChangeDetectionStrategy, inject, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';

@Component({
  selector: 'ev-header',
  standalone: true,
  imports: [RouterLink, InitialsPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="ev-header ev-glass">
      <button class="btn btn-ghost btn-icon d-lg-none" (click)="toggleMenu.emit()" aria-label="Open menu">
        <i class="bi bi-list fs-4"></i>
      </button>

      <!-- Global search -->
      <div class="ev-search ev-header__search">
        <i class="bi bi-search"></i>
        <input class="form-control" type="search" placeholder="Search students, calls, complaints…" aria-label="Search" />
        <kbd class="ev-kbd d-none d-md-inline">⌘K</kbd>
      </div>

      <div class="ev-header__actions">
        <button class="btn btn-ghost btn-icon position-relative" aria-label="Notifications">
          <i class="bi bi-bell fs-5"></i>
          <span class="ev-notif-dot"></span>
        </button>

        <div class="ev-header__divider"></div>

        <!-- User menu -->
        <div class="dropdown">
          <button class="ev-usermenu" data-bs-toggle="dropdown" aria-expanded="false">
            <div class="ev-avatar ev-avatar--sm">{{ fullName() | evInitials }}</div>
            <div class="ev-usermenu__meta d-none d-md-block">
              <div class="ev-usermenu__name">{{ fullName() }}</div>
              <div class="ev-usermenu__role">{{ roleLabel() }}</div>
            </div>
            <i class="bi bi-chevron-down ev-usermenu__caret d-none d-md-inline"></i>
          </button>
          <ul class="dropdown-menu dropdown-menu-end ev-dropdown">
            <li class="px-3 py-2">
              <div class="fw-bold">{{ fullName() }}</div>
              <div class="small text-secondary-ev">{{ user()?.email }}</div>
            </li>
            <li><hr class="dropdown-divider" /></li>
            <li><a class="dropdown-item" routerLink="/settings"><i class="bi bi-person me-2"></i>Profile</a></li>
            <li><a class="dropdown-item" routerLink="/settings"><i class="bi bi-gear me-2"></i>Settings</a></li>
            <li><hr class="dropdown-divider" /></li>
            <li>
              <button class="dropdown-item text-danger" (click)="auth.logout()">
                <i class="bi bi-box-arrow-right me-2"></i>Sign out
              </button>
            </li>
          </ul>
        </div>
      </div>
    </header>
  `,
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  readonly auth = inject(AuthService);
  readonly toggleMenu = output<void>();

  readonly user = this.auth.user;

  fullName(): string {
    const u = this.user();
    return u ? `${u.firstName} ${u.lastName}`.trim() : 'User';
  }

  roleLabel(): string {
    return (this.user()?.role ?? '').replace(/([A-Z])/g, ' $1').trim();
  }
}
