import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { StudentReportCard, AcademicReport } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly api = inject(ApiService);

  getStudentReportCard(studentId: string): Observable<StudentReportCard> {
    return this.api.get<StudentReportCard>(`reports/student/${studentId}`);
  }

  getAcademicReport(filterClass?: string): Observable<AcademicReport> {
    return this.api.get<AcademicReport>('reports/academic', filterClass ? { class_: filterClass } : undefined);
  }
}
