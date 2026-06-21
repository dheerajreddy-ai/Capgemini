import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import {
  TeacherDashboard,
  ClassSection,
  TeacherStudent,
  AttendanceResult,
  ExamType,
} from '../../core/models/models';

export interface MarkAttendanceRequest {
  date: string;
  class: string;
  section?: string;
  entries: { studentId: string; isPresent: boolean; remarks?: string }[];
}

export interface UploadMarksRequest {
  examType: ExamType;
  examDate: string;
  class: string;
  section?: string;
  maxMarksPerSubject: number;
  entries: {
    studentId: string;
    mathMarks?: number;
    scienceMarks?: number;
    englishMarks?: number;
    teluguMarks?: number;
    socialMarks?: number;
  }[];
}

export interface AssignHomeworkRequest {
  class: string;
  section?: string;
  subject: string;
  description: string;
  dueDate: string;
}

@Injectable({ providedIn: 'root' })
export class TeacherService {
  private readonly api = inject(ApiService);
  private readonly base = '/teacher';

  getDashboard(): Observable<TeacherDashboard> {
    return this.api.get<TeacherDashboard>(`${this.base}/dashboard`);
  }

  getClassSections(): Observable<ClassSection[]> {
    return this.api.get<ClassSection[]>(`${this.base}/class-sections`);
  }

  getStudents(className: string, section?: string): Observable<TeacherStudent[]> {
    const params: Record<string, string> = { className };
    if (section) params['section'] = section;
    return this.api.get<TeacherStudent[]>(`${this.base}/students`, params);
  }

  getAttendance(date: string, className: string, section?: string): Observable<AttendanceResult> {
    const params: Record<string, string> = { date, className };
    if (section) params['section'] = section;
    return this.api.get<AttendanceResult>(`${this.base}/attendance`, params);
  }

  markAttendance(request: MarkAttendanceRequest): Observable<AttendanceResult> {
    return this.api.post<AttendanceResult>(`${this.base}/attendance`, request);
  }

  uploadMarks(request: UploadMarksRequest): Observable<void> {
    return this.api.post<void>(`${this.base}/marks`, request);
  }

  assignHomework(request: AssignHomeworkRequest): Observable<void> {
    return this.api.post<void>(`${this.base}/homework`, request);
  }
}
