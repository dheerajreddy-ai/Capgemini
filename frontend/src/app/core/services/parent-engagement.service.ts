import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class ParentEngagementService {
  private readonly api = inject(ApiService);

  sendBirthdayWishes(): Observable<number> {
    return this.api.post<number>('parent-engagement/birthday-wishes', {});
  }

  sendWeeklySummaries(): Observable<number> {
    return this.api.post<number>('parent-engagement/weekly-summaries', {});
  }
}
