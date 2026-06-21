import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Broadcast, BroadcastMediaType, FeesStatus, PagedResult } from '../models/models';

export interface CreateBroadcastRequest {
  title: string;
  message: string;
  mediaUrl?: string;
  mediaType: BroadcastMediaType;
  targetClass?: string;
  targetSection?: string;
  sendNow: boolean;
}

@Injectable({ providedIn: 'root' })
export class BroadcastsService {
  private readonly api = inject(ApiService);

  list(page = 1, pageSize = 20): Observable<PagedResult<Broadcast>> {
    return this.api.get<PagedResult<Broadcast>>(`/broadcasts?page=${page}&pageSize=${pageSize}`);
  }

  get(id: string): Observable<Broadcast> {
    return this.api.get<Broadcast>(`/broadcasts/${id}`);
  }

  create(req: CreateBroadcastRequest): Observable<Broadcast> {
    return this.api.post<Broadcast>('/broadcasts', req);
  }
}
