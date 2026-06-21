import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { Sentiment } from '../../../core/models/models';

@Component({
  selector: 'ev-sentiment-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="ev-badge ev-badge--{{ variant() }}"><i class="bi {{ icon() }}"></i>{{ value() }}</span>`,
})
export class SentimentBadgeComponent {
  readonly value = input.required<Sentiment>();

  variant(): string {
    return {
      Positive: 'success',
      Neutral: 'neutral',
      Negative: 'danger',
      Angry: 'danger',
    }[this.value()];
  }

  icon(): string {
    return {
      Positive: 'bi-emoji-smile',
      Neutral: 'bi-emoji-neutral',
      Negative: 'bi-emoji-frown',
      Angry: 'bi-emoji-angry',
    }[this.value()];
  }
}
