import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  kind: ToastKind;
  title: string;
  message?: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private seq = 0;
  readonly toasts = signal<Toast[]>([]);

  success(title: string, message?: string) { this.push('success', title, message); }
  error(title: string, message?: string)   { this.push('error', title, message); }
  warning(title: string, message?: string) { this.push('warning', title, message); }
  info(title: string, message?: string)     { this.push('info', title, message); }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  private push(kind: ToastKind, title: string, message?: string): void {
    const id = ++this.seq;
    this.toasts.update((list) => [...list, { id, kind, title, message }]);
    setTimeout(() => this.dismiss(id), 4500);
  }
}
