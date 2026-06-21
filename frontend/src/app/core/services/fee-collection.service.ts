import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { FeeCollectionDashboard } from '../models/models';

@Injectable({ providedIn: 'root' })
export class FeeCollectionService {
  private readonly api = inject(ApiService);

  getDashboard(): Observable<FeeCollectionDashboard> {
    return this.api.get<FeeCollectionDashboard>('fee-collection/dashboard');
  }
}
