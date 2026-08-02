import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { AuthService } from './services/auth.service';
import { ToastContainerComponent } from './components/toast-container.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, ToastContainerComponent],
  template: `
    <app-toast-container />

    <header class="navbar">
      <a routerLink="/" class="logo">🎬 Cinema</a>
      <nav class="nav-links">
        <a routerLink="/" routerLinkActive="active">Sessões</a>

        @if (authService.isLoggedIn() && authService.isAdmin()) {
          <a routerLink="/admin" routerLinkActive="active">Admin</a>
        }

        @if (authService.isLoggedIn()) {
          <span class="user-greeting">Olá, {{ authService.getUserName() }}</span>
          <button class="btn-nav" (click)="authService.logout()">Sair</button>
        } @else {
          <a routerLink="/login" routerLinkActive="active" class="btn-login-link">Entrar</a>
        }
      </nav>
    </header>

    <main>
      <router-outlet />
    </main>
  `,
  styles: [`
    .navbar {
      background: #1a1a2e;
      color: #fff;
      padding: 0.8rem 1.5rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
      font-family: 'Segoe UI', system-ui, sans-serif;
    }

    .logo {
      color: #fff;
      text-decoration: none;
      font-size: 1.3rem;
      font-weight: 700;
    }

    .nav-links {
      display: flex;
      align-items: center;
      gap: 1.5rem;
    }

    .nav-links a {
      color: #ccc;
      text-decoration: none;
      font-size: 0.95rem;
      transition: color 0.15s;
    }

    .nav-links a:hover,
    .nav-links a.active {
      color: #fff;
    }

    .user-greeting {
      color: #aaa;
      font-size: 0.85rem;
    }

    .btn-nav {
      background: transparent;
      color: #e94560;
      border: 1px solid #e94560;
      padding: 0.35rem 0.9rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.85rem;
      transition: all 0.15s;
    }

    .btn-nav:hover {
      background: #e94560;
      color: #fff;
    }

    .btn-login-link {
      background: #e94560;
      color: #fff !important;
      padding: 0.35rem 0.9rem;
      border-radius: 6px;
      font-weight: 600 !important;
      text-decoration: none;
      transition: background 0.15s;
    }

    .btn-login-link:hover {
      background: #d63850;
    }

    main {
      min-height: calc(100vh - 52px);
    }
  `],
})
export class AppComponent {
  authService = inject(AuthService);
}
