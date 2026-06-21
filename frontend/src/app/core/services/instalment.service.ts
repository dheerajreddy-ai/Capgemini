import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { InstalmentPlan, FeeInstalment } from '../models/models';

export interface CreateInstalmentPlanRequest {
  instalmentCount: number;
  firstDueDate: string;
  intervalDays: number;
}

@Injectable({ providedIn: 'root' })
export class InstalmentService {
  private readonly api = inject(ApiService);

  getPlan(studentId: string): Observable<InstalmentPlan> {
    return this.api.get<InstalmentPlan>(`students/${studentId}/instalments`);
  }

  createPlan(studentId: string, request: CreateInstalmentPlanRequest): Observable<InstalmentPlan> {
    return this.api.post<InstalmentPlan>(`students/${studentId}/instalments`, request);
  }

  deletePlan(studentId: string): Observable<void> {
    return this.api.delete<void>(`students/${studentId}/instalments`);
  }

  markPaid(studentId: string, instalmentId: string): Observable<FeeInstalment> {
    return this.api.post<FeeInstalment>(
      `students/${studentId}/instalments/${instalmentId}/mark-paid`, {}
    );
  }
}
