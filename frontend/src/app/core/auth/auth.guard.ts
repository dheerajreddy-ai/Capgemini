import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from './token.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const token = inject(TokenService);
  const router = inject(Router);

  if (token.isAuthenticated()) return true;

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
