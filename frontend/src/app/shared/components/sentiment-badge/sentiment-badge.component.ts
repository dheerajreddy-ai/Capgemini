import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { Sentiment } from '../../../core/models/models';

@Component({
  selector: 'ev-sentiment-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `@if (value(); as v) { <span class="ev-badge ev-badge--{{ variant(v) }}"><i class="bi {{ icon(v) }}"></i>{{ v }}</span> }`,
})
export class SentimentBadgeComponent {
  readonly value = input<Sentiment | null | undefined>();

  variant(v: Sentiment): string {
    return {
      Positive: 'success',
      Neutral: 'neutral',
      Negative: 'danger',
      Angry: 'danger',
    }[v];
  }

  icon(v: Sentiment): string {
    return {
      Positive: 'bi-emoji-smile',
      Neutral: 'bi-emoji-neutral',
      Negative: 'bi-emoji-frown',
      Angry: 'bi-emoji-angry',
    }[v];
  }
}
