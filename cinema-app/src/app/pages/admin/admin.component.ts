import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <div class="admin-layout">
      <nav class="admin-nav">
        <h2>🎬 Painel Admin</h2>
        <div class="nav-links">
          <a routerLink="/admin" routerLinkActive="active">Dashboard</a>
          <a routerLink="/admin/movies" routerLinkActive="active">Filmes</a>
          <a routerLink="/admin/rooms" routerLinkActive="active">Salas</a>
          <a routerLink="/admin/sessions" routerLinkActive="active">Sessões</a>
        </div>
        <div class="nav-footer">
          <span class="user-name">{{ userName }}</span>
          <button class="btn-back" (click)="goHome()">← Voltar ao Site</button>
          <button class="btn-logout" (click)="logout()">Sair</button>
        </div>
      </nav>
      <main class="admin-content">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    .admin-layout {
      display: flex;
      min-height: 100vh;
      font-family: 'Segoe UI', system-ui, sans-serif;
    }

    .admin-nav {
      width: 250px;
      background: #1a1a2e;
      color: #fff;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
      flex-shrink: 0;
    }

    .admin-nav h2 {
      margin: 0;
      font-size: 1.2rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid rgba(255,255,255,0.2);
    }

    .nav-links {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      flex: 1;
    }

    .nav-links a {
      color: #ccc;
      text-decoration: none;
      padding: 0.6rem 0.8rem;
      border-radius: 8px;
      font-size: 0.95rem;
      transition: all 0.15s;
    }

    .nav-links a:hover {
      background: rgba(255,255,255,0.1);
      color: #fff;
    }

    .nav-links a.active {
      background: #e94560;
      color: #fff;
    }

    .nav-footer {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      padding-top: 1rem;
      border-top: 1px solid rgba(255,255,255,0.2);
    }

    .user-name {
      font-size: 0.85rem;
      color: #aaa;
      text-align: center;
    }

    .btn-back, .btn-logout {
      padding: 0.5rem;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.9rem;
      transition: background 0.15s;
    }

    .btn-back {
      background: rgba(255,255,255,0.15);
      color: #fff;
    }

    .btn-back:hover {
      background: rgba(255,255,255,0.25);
    }

    .btn-logout {
      background: #e94560;
      color: #fff;
    }

    .btn-logout:hover {
      background: #d63850;
    }

    .admin-content {
      flex: 1;
      padding: 2rem;
      background: #f0f2f5;
    }
  `]
})
export class AdminComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  userName = this.authService.getUserName();

  goHome(): void {
    this.router.navigate(['/']);
  }

  logout(): void {
    this.authService.logout();
  }
}
