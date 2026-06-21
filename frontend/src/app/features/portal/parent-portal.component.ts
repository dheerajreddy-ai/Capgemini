import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

interface PortalData {
  studentName: string;
  class: string;
  feesStatus: string;
  pendingFees: number;
  totalFees: number;
  paidFees: number;
  feesDueDate?: string;
  paymentLink?: string;
  attendancePercentage?: number;
  percentage?: number;
  grade?: string;
  mathMarks?: number;
  scienceMarks?: number;
  englishMarks?: number;
  teluguMarks?: number;
  socialMarks?: number;
  recentCalls: { type: string; status: string; aiSummary?: string; createdAt: string }[];
  openComplaints: { summary: string; status: string; createdAt: string }[];
}

type Step = 'phone' | 'otp' | 'data';

@Component({
  selector: 'ev-parent-portal',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="portal-bg">
      <div class="portal-card">
        <div class="portal-brand mb-4">
          <i class="bi bi-mortarboard-fill text-primary" style="font-size:2rem"></i>
          <h4 class="mb-0 ms-2">EduVoice Parent Portal</h4>
        </div>

        @if (step() === 'phone') {
          <h5 class="mb-1">Parent Login</h5>
          <p class="text-muted small mb-3">Enter your registered mobile number to receive an OTP.</p>
          <div class="mb-3">
            <label class="form-label">Mobile Number</label>
            <input class="form-control" type="tel" placeholder="+91 9876543210" [(ngModel)]="phone" />
          </div>
          @if (error()) { <div class="alert alert-danger py-2 small">{{ error() }}</div> }
          <button class="btn btn-primary w-100" [disabled]="busy() || !phone" (click)="sendOtp()">
            @if (busy()) { <span class="spinner-border spinner-border-sm me-2"></span> }
            Send OTP via WhatsApp
          </button>
        }

        @if (step() === 'otp') {
          <h5 class="mb-1">Verify OTP</h5>
          <p class="text-muted small mb-3">Enter the 6-digit OTP sent to {{ phone }} via WhatsApp.</p>
          <div class="mb-3">
            <label class="form-label">OTP</label>
            <input class="form-control form-control-lg text-center letter-spacing-otp" type="text"
              maxlength="6" placeholder="------" [(ngModel)]="otp" />
          </div>
          @if (error()) { <div class="alert alert-danger py-2 small">{{ error() }}</div> }
          <button class="btn btn-primary w-100 mb-2" [disabled]="busy() || otp.length < 6" (click)="verifyOtp()">
            @if (busy()) { <span class="spinner-border spinner-border-sm me-2"></span> }
            Verify & Login
          </button>
          <button class="btn btn-link btn-sm w-100" (click)="step.set('phone')">← Change number</button>
        }

        @if (step() === 'data' && data()) {
          <div class="portal-student-header mb-4">
            <div class="portal-avatar">{{ data()!.studentName[0] }}</div>
            <div>
              <h5 class="mb-0">{{ data()!.studentName }}</h5>
              <div class="text-muted small">Class {{ data()!.class }}</div>
            </div>
          </div>

          <!-- Fees -->
          <div class="portal-section">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <h6 class="mb-0"><i class="bi bi-cash-stack me-2 text-primary"></i>Fees</h6>
              <span class="badge" [class.bg-success]="data()!.feesStatus === 'Paid'"
                [class.bg-warning]="data()!.feesStatus === 'Partial'"
                [class.bg-danger]="data()!.feesStatus === 'Unpaid' || data()!.feesStatus === 'Overdue'">
                {{ data()!.feesStatus }}
              </span>
            </div>
            <div class="row g-2 text-center mb-2">
              <div class="col-4"><div class="portal-stat">₹{{ data()!.totalFees | number }}<span>Total</span></div></div>
              <div class="col-4"><div class="portal-stat text-success">₹{{ data()!.paidFees | number }}<span>Paid</span></div></div>
              <div class="col-4"><div class="portal-stat text-danger">₹{{ data()!.pendingFees | number }}<span>Due</span></div></div>
            </div>
            @if (data()!.paymentLink && data()!.pendingFees > 0) {
              <a [href]="data()!.paymentLink" class="btn btn-success w-100 btn-sm">
                <i class="bi bi-phone-fill me-2"></i>Pay ₹{{ data()!.pendingFees | number }} via UPI
              </a>
            }
          </div>

          <!-- Marks -->
          @if (data()!.percentage !== undefined) {
            <div class="portal-section">
              <h6 class="mb-2"><i class="bi bi-bar-chart me-2 text-info"></i>Academic Performance</h6>
              <div class="d-flex justify-content-between mb-2">
                <span>Overall</span>
                <strong>{{ data()!.percentage }}% ({{ data()!.grade }})</strong>
              </div>
              <div class="row g-1 text-center small">
                @for (sub of subjects(); track sub.label) {
                  <div class="col-4">
                    <div class="portal-mark-chip">{{ sub.value ?? 'N/A' }}<span>{{ sub.label }}</span></div>
                  </div>
                }
              </div>
            </div>
          }

          <!-- Attendance -->
          @if (data()!.attendancePercentage !== undefined) {
            <div class="portal-section">
              <h6 class="mb-2"><i class="bi bi-calendar-check me-2 text-success"></i>Attendance</h6>
              <div class="d-flex align-items-center gap-3">
                <div class="portal-ring" [style.--pct]="data()!.attendancePercentage! / 100">
                  {{ data()!.attendancePercentage }}%
                </div>
                <span class="text-muted small">{{ data()!.attendancePercentage! >= 75 ? 'Good attendance' : 'Low attendance — please improve' }}</span>
              </div>
            </div>
          }

          <!-- Recent Calls -->
          @if (data()!.recentCalls.length) {
            <div class="portal-section">
              <h6 class="mb-2"><i class="bi bi-telephone me-2 text-warning"></i>Recent Calls</h6>
              @for (c of data()!.recentCalls; track c.createdAt) {
                <div class="portal-call-row">
                  <div class="small fw-bold">{{ c.type }}</div>
                  <div class="badge bg-secondary">{{ c.status }}</div>
                  @if (c.aiSummary) { <div class="text-muted small mt-1">{{ c.aiSummary }}</div> }
                </div>
              }
            </div>
          }

          <button class="btn btn-outline-secondary w-100 mt-2" (click)="logout()">
            <i class="bi bi-box-arrow-right me-2"></i>Logout
          </button>
        }
      </div>
    </div>
  `,
  styles: [`
    .portal-bg { min-height: 100vh; background: linear-gradient(135deg,#667eea,#764ba2); display: flex; align-items: center; justify-content: center; padding: 1rem; }
    .portal-card { background: #fff; border-radius: 20px; padding: 2rem; width: 100%; max-width: 420px; box-shadow: 0 20px 60px rgba(0,0,0,.25); }
    .portal-brand { display: flex; align-items: center; }
    .portal-student-header { display: flex; align-items: center; gap: 1rem; background: #f8f9ff; border-radius: 14px; padding: 1rem; }
    .portal-avatar { width: 48px; height: 48px; border-radius: 50%; background: var(--ev-primary, #1E40AF); color: #fff; display: grid; place-items: center; font-size: 1.4rem; font-weight: 700; flex-shrink: 0; }
    .portal-section { border: 1px solid #eef2f8; border-radius: 14px; padding: 1rem; margin-bottom: 1rem; }
    .portal-stat { font-size: 1.25rem; font-weight: 700; display: block; }
    .portal-stat span { display: block; font-size: .7rem; color: #94a3b8; font-weight: 400; }
    .portal-mark-chip { background: #f1f5f9; border-radius: 10px; padding: .35rem .5rem; font-weight: 600; }
    .portal-mark-chip span { display: block; font-size: .65rem; color: #94a3b8; font-weight: 400; }
    .portal-ring { width: 64px; height: 64px; border-radius: 50%; border: 6px solid #e2e8f0; border-top-color: #10b981; display: grid; place-items: center; font-weight: 700; font-size: .9rem; flex-shrink: 0; }
    .portal-call-row { border-bottom: 1px solid #f1f5f9; padding: .5rem 0; }
    .portal-call-row:last-child { border-bottom: none; }
    .letter-spacing-otp { letter-spacing: .5rem; font-size: 1.5rem; }
  `],
})
export class ParentPortalComponent {
  private readonly api = inject(ApiService);

  readonly step = signal<Step>('phone');
  readonly busy = signal(false);
  readonly error = signal('');
  readonly data = signal<PortalData | null>(null);

  phone = '';
  otp = '';
  private subdomain = globalThis.location?.hostname.split('.')[0] ?? 'demo';

  sendOtp(): void {
    this.busy.set(true); this.error.set('');
    this.api.post<void>(`/inbound/portal/otp`, { phone: this.phone, subDomain: this.subdomain }).subscribe({
      next: () => { this.busy.set(false); this.step.set('otp'); },
      error: (e) => { this.busy.set(false); this.error.set(e?.error?.message ?? 'Failed to send OTP'); }
    });
  }

  verifyOtp(): void {
    this.busy.set(true); this.error.set('');
    this.api.post<PortalData>(`/inbound/portal/verify`, { phone: this.phone, otp: this.otp, subDomain: this.subdomain }).subscribe({
      next: (d) => { this.busy.set(false); this.data.set(d); this.step.set('data'); },
      error: (e) => { this.busy.set(false); this.error.set(e?.error?.message ?? 'Invalid OTP'); }
    });
  }

  logout(): void { this.step.set('phone'); this.data.set(null); this.phone = ''; this.otp = ''; }

  subjects(): { label: string; value?: number }[] {
    const d = this.data();
    if (!d) return [];
    return [
      { label: 'Maths', value: d.mathMarks },
      { label: 'Science', value: d.scienceMarks },
      { label: 'English', value: d.englishMarks },
      { label: 'Telugu', value: d.teluguMarks },
      { label: 'Social', value: d.socialMarks },
    ];
  }
}
