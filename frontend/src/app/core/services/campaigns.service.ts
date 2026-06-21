import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Call, CallingWindowStatus, Campaign, CampaignType, FeesStatus, PagedResult } from '../models/models';

export interface CreateCampaignRequest {
  name: string;
  type: CampaignType;
  description?: string;
  filterClass?: string;
  filterSection?: string;
  filterFeesStatus?: FeesStatus;
  attendanceThreshold?: number;
  customMessage?: string;
  scheduledAt?: string | null;
}

@Injectable({ providedIn: 'root' })
export class CampaignsService {
  private readonly api = inject(ApiService);

  list(page = 1, pageSize = 20): Observable<PagedResult<Campaign>> {
    return this.api.get<PagedResult<Campaign>>(`/campaigns?page=${page}&pageSize=${pageSize}`);
  }

  get(id: string, silent = false): Observable<Campaign> {
    return this.api.get<Campaign>(`/campaigns/${id}`, undefined, { silent });
  }

  create(req: CreateCampaignRequest): Observable<Campaign> {
    return this.api.post<Campaign>('/campaigns', req);
  }

  start(id: string): Observable<void> { return this.api.post<void>(`/campaigns/${id}/start`, {}); }
  pause(id: string): Observable<void> { return this.api.put<void>(`/campaigns/${id}/pause`, {}); }
  resume(id: string): Observable<void> { return this.api.put<void>(`/campaigns/${id}/resume`, {}); }
  cancel(id: string): Observable<void> { return this.api.delete<void>(`/campaigns/${id}`); }

  setDoNotCall(studentId: string): Observable<void> {
    return this.api.post<void>(`/campaigns/do-not-call/${studentId}`, {});
  }

  getCallingWindow(): Observable<CallingWindowStatus> {
    return this.api.get<CallingWindowStatus>('/campaigns/calling-window');
  }
}
