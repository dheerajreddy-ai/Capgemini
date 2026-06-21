import { Component, ChangeDetectionStrategy, input } from '@angular/core';

@Component({
  selector: 'ev-auth-shell',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-auth">
      <!-- Left: brand showcase -->
      <div class="ev-auth__brand">
        <div class="ev-auth__brand-inner">
          <div class="ev-auth__logo">
            <i class="bi bi-soundwave"></i>
            <span>EduVoice</span>
          </div>

          <h1 class="ev-auth__headline">
            AI voice agent that calls<br /> parents in <span class="ev-grad-text">Telugu</span>.
          </h1>
          <p class="ev-auth__lede">
            Automate fee reminders, share student progress, and capture complaints —
            in a natural, human-like voice your parents trust.
          </p>

          <div class="ev-auth__features">
            <div class="ev-auth__feature">
              <div class="ev-auth__feature-ico"><i class="bi bi-telephone-outbound"></i></div>
              <div><div class="fw-bold">Automated parent calls</div><small>Reach hundreds of parents in minutes</small></div>
            </div>
            <div class="ev-auth__feature">
              <div class="ev-auth__feature-ico"><i class="bi bi-translate"></i></div>
              <div><div class="fw-bold">Natural Telugu voice</div><small>Warm, human-like conversations</small></div>
            </div>
            <div class="ev-auth__feature">
              <div class="ev-auth__feature-ico"><i class="bi bi-shield-check"></i></div>
              <div><div class="fw-bold">Complete dashboard</div><small>Transcripts, recordings, insights</small></div>
            </div>
          </div>

          <div class="ev-auth__stats">
            <div><b>15,000+</b><span>Schools in AP & TS</span></div>
            <div><b>98%</b><span>Call completion</span></div>
            <div><b>4.9/5</b><span>Parent rating</span></div>
          </div>
        </div>
        <div class="ev-auth__orb ev-auth__orb--1"></div>
        <div class="ev-auth__orb ev-auth__orb--2"></div>
      </div>

      <!-- Right: form card -->
      <div class="ev-auth__panel">
        <div class="ev-auth__card ev-fade-up">
          <div class="ev-auth__logo ev-auth__logo--dark d-lg-none mb-4">
            <i class="bi bi-soundwave"></i><span>EduVoice</span>
          </div>
          <h2 class="ev-auth__title">{{ title() }}</h2>
          <p class="ev-auth__sub">{{ subtitle() }}</p>

          <div class="mt-4">
            <ng-content></ng-content>
          </div>

          <div class="ev-auth__footer">
            <ng-content select="[slot=footer]"></ng-content>
          </div>
        </div>
        <p class="ev-auth__copy">© 2026 EduVoice · VCD AI Labs · Made for schools in AP & Telangana</p>
      </div>
    </div>
  `,
  styleUrl: './auth.shared.scss',
})
export class AuthShellComponent {
  readonly title = input('Welcome');
  readonly subtitle = input('');
}
