import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthShellComponent } from '../auth-shell.component';

function matchPasswords(group: AbstractControl): ValidationErrors | null {
  const pw = group.get('password')?.value;
  const cpw = group.get('confirm')?.value;
  return pw && cpw && pw !== cpw ? { mismatch: true } : null;
}

@Component({
  selector: 'ev-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, AuthShellComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ev-auth-shell title="Set a new password" subtitle="Choose a strong password for your account.">
      <form [formGroup]="form" (ngSubmit)="submit()">
        <div class="mb-3">
          <label class="form-label">New password</label>
          <div class="ev-input-icon">
            <i class="bi bi-lock"></i>
            <input [type]="show() ? 'text' : 'password'" class="form-control" formControlName="password" placeholder="••••••••" />
            <button type="button" class="ev-input-toggle" (click)="show.set(!show())">
              <i class="bi" [class.bi-eye]="!show()" [class.bi-eye-slash]="show()"></i>
            </button>
          </div>
          @if (ctrlInvalid('password')) { <small class="ev-err">Minimum 8 characters.</small> }
        </div>
        <div class="mb-2">
          <label class="form-label">Confirm password</label>
          <div class="ev-input-icon">
            <i class="bi bi-lock-fill"></i>
            <input [type]="show() ? 'text' : 'password'" class="form-control" formControlName="confirm" placeholder="••••••••" />
          </div>
          @if (form.errors?.['mismatch'] && form.get('confirm')?.touched) {
            <small class="ev-err">Passwords do not match.</small>
          }
        </div>
        <button type="submit" class="btn btn-primary w-100 py-2 fw-bold mt-2" [disabled]="loading()">
          @if (loading()) { <span class="spinner-border spinner-border-sm me-2"></span>Updating… }
          @else { Update password }
        </button>
      </form>
      <div slot="footer" class="text-center">
        <a routerLink="/login" class="ev-link small fw-semibold"><i class="bi bi-arrow-left me-1"></i>Back to sign in</a>
      </div>
    </ev-auth-shell>
  `,
  styleUrl: '../auth.shared.scss',
})
export class ResetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly show = signal(false);
  readonly loading = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirm: ['', [Validators.required]],
    },
    { validators: matchPasswords },
  );

  ctrlInvalid(name: string): boolean {
    const c = this.form.get(name)!;
    return c.invalid && (c.dirty || c.touched);
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const token = this.route.snapshot.queryParamMap.get('token') ?? '';
    this.loading.set(true);
    this.auth.resetPassword(token, this.form.getRawValue().password).subscribe({
      next: () => {
        this.toast.success('Password updated', 'You can now sign in with your new password.');
        this.router.navigate(['/login']);
      },
      error: () => this.loading.set(false),
    });
  }
}
