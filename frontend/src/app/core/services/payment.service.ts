import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly api = inject(ApiService);

  sendLink(studentId: string): Observable<void> {
    return this.api.post<void>(`/payment/send-link/${studentId}`, {});
  }

  confirmPayment(studentId: string, amountPaid: number): Observable<void> {
    return this.api.post<void>(`/payment/confirm/${studentId}`, { amountPaid });
  }

  raiseDispute(studentId: string, note: string): Observable<void> {
    return this.api.post<void>(`/payment/dispute/${studentId}`, { note });
  }

  resolveDispute(studentId: string): Observable<void> {
    return this.api.post<void>(`/payment/resolve-dispute/${studentId}`, {});
  }

  grantExtension(studentId: string, extraDays = 14): Observable<void> {
    return this.api.post<void>(`/payment/grant-extension/${studentId}`, { extraDays });
  }
}
