import { Component, ChangeDetectionStrategy, input } from '@angular/core';

@Component({
  selector: 'ev-skeleton',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @for (row of rowsArray(); track $index) {
      <div class="ev-skeleton mb-2" [style.height.px]="height()" [style.width]="$index === rows() - 1 ? '70%' : '100%'"></div>
    }
  `,
})
export class SkeletonComponent {
  readonly rows = input(3);
  readonly height = input(16);
  rowsArray(): number[] { return Array.from({ length: this.rows() }); }
}
