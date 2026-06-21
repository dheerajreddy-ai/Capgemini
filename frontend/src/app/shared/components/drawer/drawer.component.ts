import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';

@Component({
  selector: 'ev-drawer',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (open()) {
      <div class="ev-drawer-backdrop" (click)="close.emit()"></div>
      <aside class="ev-drawer ev-drawer--{{ size() }}" role="dialog" aria-modal="true">
        <header class="ev-drawer__head">
          <div>
            <h4 class="ev-drawer__title">{{ title() }}</h4>
            @if (subtitle()) { <p class="ev-drawer__sub">{{ subtitle() }}</p> }
          </div>
          <button class="btn btn-ghost btn-icon" (click)="close.emit()" aria-label="Close"><i class="bi bi-x-lg"></i></button>
        </header>
        <div class="ev-drawer__body"><ng-content></ng-content></div>
        <footer class="ev-drawer__foot"><ng-content select="[slot=footer]"></ng-content></footer>
      </aside>
    }
  `,
  styles: [`
    .ev-drawer-backdrop { position: fixed; inset: 0; z-index: 1060; background: rgba(15,23,42,.5); backdrop-filter: blur(3px); animation: fade .2s ease; }
    .ev-drawer {
      position: fixed; top: 0; right: 0; bottom: 0; z-index: 1065;
      width: min(520px, 96vw); background: #fff; display: flex; flex-direction: column;
      box-shadow: var(--ev-shadow-xl); animation: slideIn .3s cubic-bezier(.2,.8,.2,1);
    }
    .ev-drawer--lg { width: min(680px, 96vw); }
    .ev-drawer--xl { width: min(820px, 96vw); }
    .ev-drawer__head { display: flex; align-items: flex-start; justify-content: space-between; padding: 1.4rem 1.6rem; border-bottom: 1px solid var(--ev-border); }
    .ev-drawer__title { font-size: 1.2rem; font-weight: 700; margin: 0; }
    .ev-drawer__sub { color: var(--ev-text-secondary); font-size: .85rem; margin: .2rem 0 0; }
    .ev-drawer__body { flex: 1; overflow-y: auto; padding: 1.6rem; }
    .ev-drawer__foot:empty { display: none; }
    .ev-drawer__foot { padding: 1.1rem 1.6rem; border-top: 1px solid var(--ev-border); display: flex; gap: .6rem; justify-content: flex-end; background: var(--ev-surface-2); }
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }
    @keyframes fade { from { opacity: 0; } to { opacity: 1; } }
  `],
})
export class DrawerComponent {
  readonly open = input(false);
  readonly title = input('');
  readonly subtitle = input('');
  readonly size = input<'md' | 'lg' | 'xl'>('md');
  readonly close = output<void>();
}
