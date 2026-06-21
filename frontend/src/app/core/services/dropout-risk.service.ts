import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { DropoutRiskSummary } from '../models/models';

@Injectable({ providedIn: 'root' })
export class DropoutRiskService {
  private readonly api = inject(ApiService);

  getSummary(level?: string): Observable<DropoutRiskSummary> {
    const params = level ? `?level=${level}` : '';
    return this.api.get<DropoutRiskSummary>(`/dropout-risk${params}`);
  }

  recalculate(): Observable<{ success: boolean; message: string }> {
    return this.api.post('/dropout-risk/recalculate', {});
  }
}
