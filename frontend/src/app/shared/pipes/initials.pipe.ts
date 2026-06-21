import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'evInitials', standalone: true })
export class InitialsPipe implements PipeTransform {
  transform(name: string | null | undefined): string {
    if (!name) return '?';
    const parts = name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }
}
