import { Injectable, inject, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, map, BehaviorSubject, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';
import { ApiResponse, AuthResult, School, UserRole } from '../models/models';
import { BrandingService } from '../services/branding.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly token = inject(TokenService);
  private readonly router = inject(Router);
  private readonly branding = inject(BrandingService);
  private readonly base = `${environment.apiUrl}/auth`;

  /** Used by the interceptor to single-flight refresh calls. */
  private refreshInFlight$: Observable<string> | null = null;
  readonly refreshing$ = new BehaviorSubject<boolean>(false);

  readonly user = this.token.user;
  readonly school = this.token.school;
  readonly isAuthenticated = this.token.isAuthenticated;
  readonly role = computed<UserRole | null>(() => this.token.user()?.role ?? null);
  readonly isSuperAdmin = computed(() => this.role() === 'SuperAdmin');

  login(email: string, password: string): Observable<AuthResult> {
    return this.http
      .post<ApiResponse<AuthResult>>(`${this.base}/login`, { email, password })
      .pipe(map((r) => this.unwrap(r)), tap((auth) => this.applySession(auth)));
  }

  registerSchool(payload: {
    schoolName: string;
    subDomain: string;
    adminEmail: string;
    adminPassword: string;
    contactPhone: string;
    city: string;
    planType: string;
  }): Observable<AuthResult> {
    return this.http
      .post<ApiResponse<AuthResult>>(`${this.base}/register-school`, payload)
      .pipe(map((r) => this.unwrap(r)), tap((auth) => this.applySession(auth)));
  }

  forgotPassword(email: string): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.base}/forgot-password`, { email })
      .pipe(map(() => void 0));
  }

  resetPassword(token: string, newPassword: string): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.base}/reset-password`, { token, newPassword })
      .pipe(map(() => void 0));
  }

  refreshToken(): Observable<string> {
    if (this.refreshInFlight$) return this.refreshInFlight$;
    this.refreshing$.next(true);

    this.refreshInFlight$ = this.http
      .post<ApiResponse<AuthResult>>(`${this.base}/refresh`, { refreshToken: this.token.refreshToken })
      .pipe(
        map((r) => this.unwrap(r)),
        tap((auth) => this.applySession(auth)),
        map((auth) => auth.accessToken),
        tap({
          next: () => {
            this.refreshInFlight$ = null;
            this.refreshing$.next(false);
          },
          error: () => {
            this.refreshInFlight$ = null;
            this.refreshing$.next(false);
          },
        }),
        catchError((err) => {
          this.forceLogout();
          return throwError(() => err);
        }),
      );
    return this.refreshInFlight$;
  }

  logout(): void {
    this.http.post(`${this.base}/logout`, {}).subscribe({ next: () => {}, error: () => {} });
    this.forceLogout();
  }

  forceLogout(): void {
    this.token.clear();
    this.router.navigate(['/login']);
  }

  private applySession(auth: AuthResult): void {
    this.token.setSession(auth.accessToken, auth.refreshToken, auth.user, auth.school);
    if (auth.school) this.branding.apply(auth.school as School);
  }

  private unwrap(res: ApiResponse<AuthResult>): AuthResult {
    if (!res.success || !res.data) throw new Error(res.message || 'Authentication failed');
    return res.data;
  }
}
