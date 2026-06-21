import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { School } from '../models/models';

export interface AdminSchool extends School {
  totalCalls: number;
  totalStudents: number;
  monthlyRevenue: number;
  status: 'Active' | 'Trial' | 'Inactive';
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly api = inject(ApiService);

  listSchools(): Observable<AdminSchool[]> {
    return this.api.get<AdminSchool[]>('/admin/schools');
  }

  createSchool(payload: Partial<School> & { adminEmail: string; adminPassword: string }): Observable<School> {
    return this.api.post<School>('/admin/schools', payload);
  }

  updateSchool(id: string, payload: Partial<School>): Observable<School> {
    return this.api.put<School>(`/admin/schools/${id}`, payload);
  }

  deactivateSchool(id: string): Observable<void> {
    return this.api.delete<void>(`/admin/schools/${id}`);
  }
}
