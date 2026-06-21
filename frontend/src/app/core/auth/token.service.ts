import { Injectable, signal, computed } from '@angular/core';
import { User, School } from '../models/models';

const REFRESH_KEY = 'ev_rt';
const USER_KEY = 'ev_user';
const SCHOOL_KEY = 'ev_school';

/**
 * Holds the access token in memory only (never localStorage) for security.
 * The refresh token is persisted so the session survives reloads; in
 * production the backend issues it as an httpOnly cookie and this falls back.
 */
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly _accessToken = signal<string | null>(null);
  private readonly _user = signal<User | null>(this.read<User>(USER_KEY));
  private readonly _school = signal<School | null>(this.read<School>(SCHOOL_KEY));

  readonly user = this._user.asReadonly();
  readonly school = this._school.asReadonly();
  readonly isAuthenticated = computed(() => !!this._user());

  get accessToken(): string | null {
    return this._accessToken();
  }

  get refreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY);
  }

  setSession(accessToken: string, refreshToken: string, user: User, school?: School): void {
    this._accessToken.set(accessToken);
    this._user.set(user);
    localStorage.setItem(REFRESH_KEY, refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    if (school) {
      this._school.set(school);
      localStorage.setItem(SCHOOL_KEY, JSON.stringify(school));
    }
  }

  setAccessToken(token: string): void {
    this._accessToken.set(token);
  }

  updateSchool(school: School): void {
    this._school.set(school);
    localStorage.setItem(SCHOOL_KEY, JSON.stringify(school));
  }

  clear(): void {
    this._accessToken.set(null);
    this._user.set(null);
    this._school.set(null);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(SCHOOL_KEY);
  }

  private read<T>(key: string): T | null {
    try {
      const raw = localStorage.getItem(key);
      return raw ? (JSON.parse(raw) as T) : null;
    } catch {
      return null;
    }
  }
}
