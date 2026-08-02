import { inject } from '@angular/core';
import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse
} from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Interceptor global de erros HTTP:
 * - 401 Unauthorized → tenta refresh do token; se falhar, redireciona para /login
 * - Demais erros → log no console e propaga
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !req.url.includes('/auth/refresh')) {
        // Tenta renovar o token automaticamente
        return authService.refreshToken().pipe(
          switchMap((response) => {
            // Re-clona a requisição original com o novo token
            const newReq = req.clone({
              setHeaders: { Authorization: `Bearer ${response.token}` }
            });
            return next(newReq);
          }),
          catchError(() => {
            // Refresh falhou — força novo login
            localStorage.removeItem('jwt_token');
            localStorage.removeItem('refresh_token');
            localStorage.removeItem('user_info');
            router.navigate(['/login']);
            return throwError(() => err);
          })
        );
      }

      console.error(`[HTTP ${err.status}] ${req.method} ${req.url}`, err);
      return throwError(() => err);
    })
  );
};
