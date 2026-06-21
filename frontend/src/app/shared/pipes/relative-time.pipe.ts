import { Pipe, PipeTransform } from '@angular/core';
import { formatDistanceToNowStrict, isValid, parseISO } from 'date-fns';

@Pipe({ name: 'evRelativeTime', standalone: true })
export class RelativeTimePipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) return '—';
    const date = typeof value === 'string' ? parseISO(value) : value;
    if (!isValid(date)) return '—';
    return formatDistanceToNowStrict(date, { addSuffix: true });
  }
}
