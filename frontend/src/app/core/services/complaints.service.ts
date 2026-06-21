import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Complaint, PagedResult } from '../models/models';

export interface ComplaintQuery {
  page?: number;
  pageSize?: number;
  status?: string;
  priority?: string;
  category?: string;
  fromDate?: string;
  toDate?: string;
}

@Injectable({ providedIn: 'root' })
export class ComplaintsService {
  private readonly api = inject(ApiService);

  list(q: ComplaintQuery): Observable<PagedResult<Complaint>> {
    return this.api.get<PagedResult<Complaint>>('/complaints', { ...q });
  }

  get(id: string): Observable<Complaint> {
    return this.api.get<Complaint>(`/complaints/${id}`);
  }

  update(id: string, payload: Partial<Complaint>): Observable<Complaint> {
    return this.api.put<Complaint>(`/complaints/${id}`, payload);
  }

  export(): Observable<Blob> {
    return this.api.download('/complaints/export');
  }
}
