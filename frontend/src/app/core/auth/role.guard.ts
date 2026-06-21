import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from './token.service';
import { UserRole } from '../models/models';

export function roleGuard(allowed: UserRole[]): CanActivateFn {
  return () => {
    const token = inject(TokenService);
    const router = inject(Router);
    const role = token.user()?.role;

    if (role && allowed.includes(role)) return true;

    return router.createUrlTree(['/dashboard']);
  };
}
