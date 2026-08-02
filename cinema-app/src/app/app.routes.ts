import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';

export const routes: Routes = [
  // ── Público ─────────────────────────────────────
  {
    path: '',
    loadComponent: () =>
      import('./pages/sessions/session-list.component').then(
        (m) => m.SessionListComponent
      ),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then(
        (m) => m.LoginComponent
      ),
  },

  // ── User (requer login) ─────────────────────────
  {
    path: 'booking/:id',
    loadComponent: () =>
      import('./pages/booking/booking.component').then(
        (m) => m.BookingComponent
      ),
    canActivate: [authGuard],
  },

  // ── Admin (requer login + perfil Admin) ─────────
  {
    path: 'admin',
    loadComponent: () =>
      import('./pages/admin/admin.component').then(
        (m) => m.AdminComponent
      ),
    canActivate: [adminGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/admin/dashboard/admin-dashboard.component').then(
            (m) => m.AdminDashboardComponent
          ),
      },
      {
        path: 'movies',
        loadComponent: () =>
          import('./pages/admin/movie-form/movie-form.component').then(
            (m) => m.MovieFormComponent
          ),
      },
      {
        path: 'rooms',
        loadComponent: () =>
          import('./pages/admin/room-form/room-form.component').then(
            (m) => m.RoomFormComponent
          ),
      },
      {
        path: 'sessions',
        loadComponent: () =>
          import('./pages/admin/session-form/session-form.component').then(
            (m) => m.SessionFormComponent
          ),
      },
      {
        path: 'sessions/:id/seats',
        loadComponent: () =>
          import('./pages/admin/session-seats/session-seats.component').then(
            (m) => m.AdminSessionSeatsComponent
          ),
      },
    ],
  },

  // ── Fallback ────────────────────────────────────
  {
    path: '**',
    redirectTo: '',
  },
];
