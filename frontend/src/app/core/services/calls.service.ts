import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Call, PagedResult } from '../models/models';

export interface CallQuery {
  page?: number;
  pageSize?: number;
  campaignId?: string;
  studentId?: string;
  callType?: string;
  status?: string;
  sentiment?: string;
  fromDate?: string;
  toDate?: string;
  hasComplaint?: boolean;
  search?: string;
}

@Injectable({ providedIn: 'root' })
export class CallsService {
  private readonly api = inject(ApiService);

  list(q: CallQuery): Observable<PagedResult<Call>> {
    return this.api.get<PagedResult<Call>>('/calls', { ...q });
  }

  get(id: string): Observable<Call> {
    return this.api.get<Call>(`/calls/${id}`);
  }

  retry(id: string): Observable<void> {
    return this.api.post<void>(`/calls/${id}/retry`, {});
  }
}
