import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastHostComponent } from './shared/components/toast/toast-host.component';
import { LoadingBarComponent } from './shared/components/loading-bar/loading-bar.component';

@Component({
  selector: 'ev-root',
  standalone: true,
  imports: [RouterOutlet, ToastHostComponent, LoadingBarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ev-loading-bar />
    <router-outlet />
    <ev-toast-host />
  `,
})
export class AppComponent {}
