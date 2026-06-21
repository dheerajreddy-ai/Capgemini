import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      // 401 is handled by the auth interceptor (silent refresh).
      if (err.status === 401) return throwError(() => err);

      let message = 'Something went wrong. Please try again.';
      if (err.status === 0) message = 'Network error — check your connection.';
      else if (err.status === 403) message = 'You do not have permission to do that.';
      else if (err.status === 404) message = 'The requested resource was not found.';
      else if (err.error?.message) message = err.error.message;

      // Don't toast on background/polling requests.
      if (!req.headers.has('X-Silent')) toast.error(message);

      return throwError(() => err);
    }),
  );
};
