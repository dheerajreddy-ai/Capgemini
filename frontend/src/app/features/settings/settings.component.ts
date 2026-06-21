import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../core/services/settings.service';
import { AuthService } from '../../core/auth/auth.service';
import { BrandingService } from '../../core/services/branding.service';
import { ToastService } from '../../core/services/toast.service';
import { School, User, ComplaintSlaConfig } from '../../core/models/models';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { InitialsPipe } from '../../shared/pipes/initials.pipe';

@Component({
  selector: 'ev-settings',
  standalone: true,
  imports: [FormsModule, StatusBadgeComponent, InitialsPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent implements OnInit {
  private readonly service = inject(SettingsService);
  private readonly auth = inject(AuthService);
  private readonly branding = inject(BrandingService);
  private readonly toast = inject(ToastService);

  readonly tab = signal<'profile' | 'users' | 'voice' | 'notifications' | 'security' | 'complaint-sla' | 'daily-digest'>('profile');
  readonly tabs = [
    { id: 'profile', label: 'School Profile', icon: 'bi-building' },
    { id: 'users', label: 'Users', icon: 'bi-people' },
    { id: 'voice', label: 'Voice', icon: 'bi-soundwave' },
    { id: 'notifications', label: 'Notifications', icon: 'bi-bell' },
    { id: 'security', label: 'Security', icon: 'bi-shield-lock' },
    { id: 'complaint-sla', label: 'Complaint SLA', icon: 'bi-clock-history' },
    { id: 'daily-digest', label: 'Daily Digest', icon: 'bi-newspaper' },
  ] as const;

  readonly school = signal<School | null>(null);
  readonly users = signal<User[]>([]);
  readonly slaConfigs = signal<ComplaintSlaConfig[]>([]);
  readonly saving = signal(false);
  readonly slaSaving = signal(false);

  pwd = { current: '', next: '', confirm: '' };
  notif = { whatsapp: true, email: true, complaintAlert: true, dailySummary: '18:00' };

  ngOnInit(): void {
    this.service.getSchool().subscribe((s) => this.school.set(s));
    this.service.listUsers().subscribe((u) => this.users.set(u));
    this.service.getSlaConfigs().subscribe((configs) => this.slaConfigs.set(configs));
  }

  saveProfile(): void {
    const s = this.school();
    if (!s) return;
    this.saving.set(true);
    this.service.updateSchool(s).subscribe({
      next: (updated) => {
        this.saving.set(false);
        this.branding.apply(updated);
        this.toast.success('Saved', 'School profile updated.');
      },
      error: () => this.saving.set(false),
    });
  }

  onColorChange(value: string): void {
    const s = this.school();
    if (s) { this.school.set({ ...s, primaryColor: value }); this.branding.apply({ ...s, primaryColor: value }); }
  }

  changePassword(): void {
    if (this.pwd.next !== this.pwd.confirm) { this.toast.warning('Mismatch', 'Passwords do not match.'); return; }
    if (this.pwd.next.length < 8) { this.toast.warning('Too short', 'Use at least 8 characters.'); return; }
    this.saving.set(true);
    this.service.changePassword(this.pwd.current, this.pwd.next).subscribe({
      next: () => { this.saving.set(false); this.pwd = { current: '', next: '', confirm: '' }; this.toast.success('Password changed'); },
      error: () => this.saving.set(false),
    });
  }

  saveNotifications(): void { this.toast.success('Preferences saved', 'Notification settings updated.'); }

  saveDigestSettings(): void {
    const s = this.school();
    if (!s) return;
    this.saving.set(true);
    this.service.updateSchool(s).subscribe({
      next: (updated) => { this.school.set(updated); this.saving.set(false); this.toast.success('Saved', 'Daily digest settings updated.'); },
      error: () => this.saving.set(false),
    });
  }

  saveSlaConfig(config: ComplaintSlaConfig): void {
    this.slaSaving.set(true);
    this.service.upsertSlaConfig(config.category, config.slaHours, config.escalationContactUserId).subscribe({
      next: (updated) => {
        this.slaConfigs.update((list) => list.map((c) => c.category === updated.category ? updated : c));
        this.slaSaving.set(false);
        this.toast.success('Saved', `SLA for ${config.category} updated.`);
      },
      error: () => this.slaSaving.set(false),
    });
  }

  fullName(u: User): string { return `${u.firstName} ${u.lastName}`.trim(); }
}
