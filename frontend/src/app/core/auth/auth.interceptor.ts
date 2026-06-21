import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { catchError, switchMap, throwError, Observable } from 'rxjs';
import { TokenService } from './token.service';
import { AuthService } from './auth.service';

const AUTH_FREE = ['/auth/login', '/auth/refresh', '/auth/register-school', '/auth/forgot-password', '/auth/reset-password'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(TokenService);
  const auth = inject(AuthService);

  const withAuth = attachToken(req, token.accessToken);

  return next(withAuth).pipe(
    catchError((err: HttpErrorResponse) => {
      const isAuthCall = AUTH_FREE.some((u) => req.url.includes(u));
      if (err.status === 401 && !isAuthCall && token.refreshToken) {
        return retryWithRefresh(req, next, auth, token);
      }
      return throwError(() => err);
    }),
  );
};

function attachToken(req: HttpRequest<unknown>, accessToken: string | null): HttpRequest<unknown> {
  if (!accessToken) return req;
  return req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } });
}

function retryWithRefresh(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  auth: AuthService,
  token: TokenService,
): Observable<any> {
  return auth.refreshToken().pipe(
    switchMap((newToken) => next(attachToken(req, newToken))),
    catchError((err) => {
      auth.forceLogout();
      return throwError(() => err);
    }),
  );
}
