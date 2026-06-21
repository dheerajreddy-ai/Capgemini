import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpContext, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/models';

export interface QueryParams {
  [key: string]: string | number | boolean | undefined | null;
}

/** Thin base HTTP service that unwraps the standard ApiResponse envelope. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  get<T>(path: string, query?: QueryParams, opts?: { silent?: boolean }): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(`${this.base}${path}`, { params: this.params(query), headers: this.headers(opts) })
      .pipe(map((r) => this.unwrap(r)));
  }

  post<T>(path: string, body: unknown, opts?: { silent?: boolean }): Observable<T> {
    return this.http
      .post<ApiResponse<T>>(`${this.base}${path}`, body, { headers: this.headers(opts) })
      .pipe(map((r) => this.unwrap(r)));
  }

  put<T>(path: string, body: unknown): Observable<T> {
    return this.http.put<ApiResponse<T>>(`${this.base}${path}`, body).pipe(map((r) => this.unwrap(r)));
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<ApiResponse<T>>(`${this.base}${path}`).pipe(map((r) => this.unwrap(r)));
  }

  /** For file (Excel/PDF) downloads — returns a Blob. */
  download(path: string, query?: QueryParams): Observable<Blob> {
    return this.http.get(`${this.base}${path}`, { params: this.params(query), responseType: 'blob' });
  }

  /** For multipart uploads (Excel import). */
  upload<T>(path: string, form: FormData): Observable<T> {
    return this.http.post<ApiResponse<T>>(`${this.base}${path}`, form).pipe(map((r) => this.unwrap(r)));
  }

  private params(query?: QueryParams): HttpParams {
    let params = new HttpParams();
    if (query) {
      for (const [k, v] of Object.entries(query)) {
        if (v !== undefined && v !== null && v !== '') params = params.set(k, String(v));
      }
    }
    return params;
  }

  private headers(opts?: { silent?: boolean }): HttpHeaders | undefined {
    return opts?.silent ? new HttpHeaders({ 'X-Silent': '1' }) : undefined;
  }

  private unwrap<T>(res: ApiResponse<T>): T {
    if (!res.success) throw new Error(res.message || 'Request failed');
    return res.data as T;
  }
}
