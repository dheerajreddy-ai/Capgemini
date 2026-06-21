import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ExamSchedule, ExamType, PagedResult } from '../models/models';

export interface CreateExamScheduleRequest {
  subjectName: string;
  examType: ExamType;
  examDate: string;
  class?: string;
  section?: string;
  notes?: string;
}

@Injectable({ providedIn: 'root' })
export class ExamScheduleService {
  private readonly api = inject(ApiService);

  list(page = 1, pageSize = 50): Observable<PagedResult<ExamSchedule>> {
    return this.api.get<PagedResult<ExamSchedule>>(`/exam-schedules?page=${page}&pageSize=${pageSize}`);
  }

  create(req: CreateExamScheduleRequest): Observable<ExamSchedule> {
    return this.api.post<ExamSchedule>('/exam-schedules', req);
  }

  delete(id: string): Observable<boolean> {
    return this.api.delete<boolean>(`/exam-schedules/${id}`);
  }
}
