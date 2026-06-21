import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthShellComponent } from '../auth-shell.component';

@Component({
  selector: 'ev-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthShellComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ev-auth-shell
      title="Reset your password"
      subtitle="Enter your email and we'll send you a reset link.">
      @if (!sent()) {
        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="mb-3">
            <label class="form-label">Email address</label>
            <div class="ev-input-icon">
              <i class="bi bi-envelope"></i>
              <input type="email" class="form-control" formControlName="email" placeholder="you@school.edu" />
            </div>
            @if (invalid()) { <small class="ev-err">Enter a valid email address.</small> }
          </div>
          <button type="submit" class="btn btn-primary w-100 py-2 fw-bold" [disabled]="loading()">
            @if (loading()) { <span class="spinner-border spinner-border-sm me-2"></span>Sending… }
            @else { Send reset link }
          </button>
        </form>
      } @else {
        <div class="ev-auth-success text-center">
          <i class="bi bi-check-circle-fill d-block fs-2 mb-2"></i>
          <div class="fw-bold mb-1">Check your inbox</div>
          <div>If an account exists for that email, a reset link is on its way.</div>
        </div>
      }

      <div slot="footer" class="text-center">
        <a routerLink="/login" class="ev-link small fw-semibold"><i class="bi bi-arrow-left me-1"></i>Back to sign in</a>
      </div>
    </ev-auth-shell>
  `,
  styleUrl: '../auth.shared.scss',
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly loading = signal(false);
  readonly sent = signal(false);
  readonly form = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]] });

  invalid(): boolean {
    const c = this.form.get('email')!;
    return c.invalid && (c.dirty || c.touched);
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.auth.forgotPassword(this.form.getRawValue().email).subscribe({
      next: () => { this.loading.set(false); this.sent.set(true); },
      error: () => { this.loading.set(false); this.sent.set(true); }, // do not leak account existence
    });
  }
}
