import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { finalize } from 'rxjs';
import { LoadingService } from '../services/loading.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loading = inject(LoadingService);
  // Skip the noisy polling endpoints so the top bar doesn't flicker.
  const silent = req.headers.has('X-Silent');
  if (!silent) loading.start();

  return next(req).pipe(finalize(() => !silent && loading.stop()));
};
