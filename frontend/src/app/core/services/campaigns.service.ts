import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Call, Campaign, PagedResult } from '../models/models';

export interface CreateCampaignRequest {
  campaignName: string;
  campaignType: 'FeeReminder' | 'ProgressUpdate';
  filters: {
    classes: string[];
    feesStatus: string;
    belowMarksThreshold?: number;
    specificStudentIds: string[];
  };
  scheduledAt?: string | null;
}

@Injectable({ providedIn: 'root' })
export class CampaignsService {
  private readonly api = inject(ApiService);

  list(): Observable<Campaign[]> {
    return this.api.get<Campaign[]>('/campaigns');
  }

  get(id: string, silent = false): Observable<Campaign & { calls: PagedResult<Call> }> {
    return this.api.get(`/campaigns/${id}`, undefined, { silent });
  }

  preview(req: CreateCampaignRequest): Observable<{ count: number }> {
    return this.api.post<{ count: number }>('/campaigns/preview', req);
  }

  create(req: CreateCampaignRequest): Observable<Campaign> {
    return this.api.post<Campaign>('/campaigns', req);
  }

  pause(id: string): Observable<void> { return this.api.put<void>(`/campaigns/${id}/pause`, {}); }
  resume(id: string): Observable<void> { return this.api.put<void>(`/campaigns/${id}/resume`, {}); }
  cancel(id: string): Observable<void> { return this.api.delete<void>(`/campaigns/${id}`); }
}
