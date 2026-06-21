import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { StaffAbsence, PtmSchedule } from '../models/models';

export interface LogAbsenceRequest {
  teacherName: string;
  teacherPhone?: string;
  substituteTeacherName?: string;
  substituteTeacherPhone?: string;
  affectedClass?: string;
  affectedSection?: string;
  absenceDate: string;
  notes?: string;
  notifyParents: boolean;
}

export interface CreatePtmRequest {
  title: string;
  ptmDate: string;
  class?: string;
  section?: string;
  notes?: string;
}

@Injectable({ providedIn: 'root' })
export class StaffOperationsService {
  private readonly api = inject(ApiService);

  getAbsences(date?: string): Observable<StaffAbsence[]> {
    return this.api.get<StaffAbsence[]>('staff-operations/absences', date ? { date } : undefined);
  }

  logAbsence(request: LogAbsenceRequest): Observable<StaffAbsence> {
    return this.api.post<StaffAbsence>('staff-operations/absences', request);
  }

  getPtmSchedules(): Observable<PtmSchedule[]> {
    return this.api.get<PtmSchedule[]>('staff-operations/ptm');
  }

  schedulePtm(request: CreatePtmRequest): Observable<PtmSchedule> {
    return this.api.post<PtmSchedule>('staff-operations/ptm', request);
  }

  deletePtm(ptmId: string): Observable<void> {
    return this.api.delete<void>(`staff-operations/ptm/${ptmId}`);
  }
}
