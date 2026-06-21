import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { School, User } from '../models/models';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly api = inject(ApiService);

  getSchool(): Observable<School> {
    return this.api.get<School>('/settings');
  }

  updateSchool(payload: Partial<School>): Observable<School> {
    return this.api.put<School>('/settings', payload);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.api.put<void>('/settings/password', { currentPassword, newPassword });
  }

  listUsers(): Observable<User[]> {
    return this.api.get<User[]>('/users');
  }

  inviteUser(payload: { email: string; firstName: string; lastName: string; role: string }): Observable<User> {
    return this.api.post<User>('/users', payload);
  }

  updateUser(id: string, payload: Partial<User>): Observable<User> {
    return this.api.put<User>(`/users/${id}`, payload);
  }

  deactivateUser(id: string): Observable<void> {
    return this.api.delete<void>(`/users/${id}`);
  }
}
