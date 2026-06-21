import { Component, ChangeDetectionStrategy, input } from '@angular/core';

@Component({
  selector: 'ev-empty-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ev-empty">
      <div class="ev-empty__icon"><i class="bi {{ icon() }}"></i></div>
      <h5 class="mb-1">{{ title() }}</h5>
      <p class="mb-0 text-secondary-ev">{{ message() }}</p>
      <ng-content></ng-content>
    </div>
  `,
})
export class EmptyStateComponent {
  readonly icon = input('bi-inbox');
  readonly title = input('Nothing here yet');
  readonly message = input('Data will appear here once available.');
}
