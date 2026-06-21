import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthShellComponent } from '../auth-shell.component';

@Component({
  selector: 'ev-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthShellComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ev-auth-shell
      title="Welcome back"
      subtitle="Sign in to manage your school's AI parent calls.">
      <form [formGroup]="form" (ngSubmit)="submit()" class="ev-auth-form">
        <div class="mb-3">
          <label class="form-label">Email address</label>
          <div class="ev-input-icon">
            <i class="bi bi-envelope"></i>
            <input type="email" class="form-control" formControlName="email"
                   placeholder="you@school.edu" autocomplete="email" />
          </div>
          @if (invalid('email')) { <small class="ev-err">Enter a valid email address.</small> }
        </div>

        <div class="mb-2">
          <div class="d-flex justify-content-between align-items-center mb-1">
            <label class="form-label mb-0">Password</label>
            <a routerLink="/forgot-password" class="ev-link small">Forgot?</a>
          </div>
          <div class="ev-input-icon">
            <i class="bi bi-lock"></i>
            <input [type]="showPassword() ? 'text' : 'password'" class="form-control"
                   formControlName="password" placeholder="••••••••" autocomplete="current-password" />
            <button type="button" class="ev-input-toggle" (click)="showPassword.set(!showPassword())"
                    [attr.aria-label]="showPassword() ? 'Hide password' : 'Show password'">
              <i class="bi" [class.bi-eye]="!showPassword()" [class.bi-eye-slash]="showPassword()"></i>
            </button>
          </div>
          @if (invalid('password')) { <small class="ev-err">Password is required.</small> }
        </div>

        @if (error()) {
          <div class="ev-auth-alert"><i class="bi bi-exclamation-circle me-2"></i>{{ error() }}</div>
        }

        <button type="submit" class="btn btn-primary w-100 mt-3 py-2 fw-bold" [disabled]="loading()">
          @if (loading()) {
            <span class="spinner-border spinner-border-sm me-2"></span>Signing in…
          } @else {
            Sign in <i class="bi bi-arrow-right ms-1"></i>
          }
        </button>
      </form>

      <div slot="footer" class="text-center">
        <span class="text-secondary-ev small">New to EduVoice?</span>
        <a routerLink="/forgot-password" class="ev-link small fw-semibold ms-1">Contact your administrator</a>
      </div>
    </ev-auth-shell>
  `,
  styleUrl: '../auth.shared.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  readonly showPassword = signal(false);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  invalid(ctrl: string): boolean {
    const c = this.form.get(ctrl);
    return !!c && c.invalid && (c.dirty || c.touched);
  }

  submit(): void {
    this.error.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe({
      next: () => {
        this.toast.success('Signed in', 'Welcome back to EduVoice.');
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/dashboard';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message ?? 'Invalid email or password.');
      },
    });
  }
}
