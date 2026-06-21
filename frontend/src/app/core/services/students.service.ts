import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { PagedResult, Student } from '../models/models';

export interface StudentQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  class?: string;
  section?: string;
  feesStatus?: string;
  sortBy?: string;
}

@Injectable({ providedIn: 'root' })
export class StudentsService {
  private readonly api = inject(ApiService);

  list(q: StudentQuery): Observable<PagedResult<Student>> {
    return this.api.get<PagedResult<Student>>('/students', { ...q });
  }

  get(id: string): Observable<Student> {
    return this.api.get<Student>(`/students/${id}`);
  }

  create(payload: Partial<Student>): Observable<Student> {
    return this.api.post<Student>('/students', payload);
  }

  update(id: string, payload: Partial<Student>): Observable<Student> {
    return this.api.put<Student>(`/students/${id}`, payload);
  }

  remove(id: string): Observable<void> {
    return this.api.delete<void>(`/students/${id}`);
  }

  import(file: File): Observable<{ imported: number; failed: number; errors: string[] }> {
    const form = new FormData();
    form.append('file', file);
    return this.api.upload('/students/import', form);
  }

  export(): Observable<Blob> {
    return this.api.download('/students/export');
  }
}
