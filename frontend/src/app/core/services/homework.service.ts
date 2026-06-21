import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Homework, PagedResult } from '../models/models';

export interface CreateHomeworkRequest {
  subject: string;
  description: string;
  class?: string;
  section?: string;
  assignedDate: string;
  dueDate?: string;
}

@Injectable({ providedIn: 'root' })
export class HomeworkService {
  private readonly api = inject(ApiService);

  list(page = 1, pageSize = 30, date?: string): Observable<PagedResult<Homework>> {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (date) params.set('date', date);
    return this.api.get<PagedResult<Homework>>(`/homework?${params}`);
  }

  create(req: CreateHomeworkRequest): Observable<Homework> {
    return this.api.post<Homework>('/homework', req);
  }

  delete(id: string): Observable<boolean> {
    return this.api.delete<boolean>(`/homework/${id}`);
  }
}
