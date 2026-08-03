import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="admin-layout" [class.sidebar-open]="isSidebarOpen">
      <!-- Mobile Top Bar -->
      <header class="mobile-topbar">
        <button class="hamburger" (click)="toggleSidebar()" aria-label="Menu">
          @if (isSidebarOpen) {
            <span class="hamburger-icon">✕</span>
          } @else {
            <span class="hamburger-icon">☰</span>
          }
        </button>
        <span class="topbar-title">🎬 Painel Admin</span>
        <span class="topbar-user">{{ userName }}</span>
      </header>

      <!-- Backdrop (mobile only) -->
      <div class="sidebar-backdrop" (click)="closeSidebar()"></div>

      <!-- Sidebar -->
      <nav class="admin-nav">
        <div class="nav-header">
          <h2>🎬 Painel Admin</h2>
        </div>
        <div class="nav-links">
          <a routerLink="/admin" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" (click)="closeSidebar()">📊 Dashboard</a>
          <a routerLink="/admin/movies" routerLinkActive="active" (click)="closeSidebar()">🎥 Filmes</a>
          <a routerLink="/admin/rooms" routerLinkActive="active" (click)="closeSidebar()">🏠 Salas</a>
          <a routerLink="/admin/sessions" routerLinkActive="active" (click)="closeSidebar()">📅 Sessões</a>
        </div>
        <div class="nav-footer">
          <span class="user-name">{{ userName }}</span>
          <button class="btn-back" (click)="goHome()">← Voltar ao Site</button>
          <button class="btn-logout" (click)="logout()">Sair</button>
        </div>
      </nav>

      <!-- Main Content -->
      <main class="admin-content">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    /* ═══════════════════════════════════════════
       Base Layout (Desktop-first)
       ═══════════════════════════════════════════ */
    .admin-layout {
      display: flex;
      min-height: 100vh;
      font-family: 'Segoe UI', system-ui, sans-serif;
    }

    /* ── Mobile Topbar (hidden on desktop) ─── */
    .mobile-topbar {
      display: none;
    }

    .sidebar-backdrop {
      display: none;
    }

    /* ── Sidebar ──────────────────────────── */
    .admin-nav {
      width: 250px;
      background: #1a1a2e;
      color: #fff;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
      flex-shrink: 0;
      z-index: 100;
    }

    .nav-header h2 {
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

    /* ── Content ─────────────────────────── */
    .admin-content {
      flex: 1;
      padding: 2rem;
      background: #f0f2f5;
      overflow-x: hidden;
    }

    /* ═══════════════════════════════════════════
       Mobile (≤768px)
       ═══════════════════════════════════════════ */
    @media (max-width: 768px) {
      .admin-layout {
        flex-direction: column;
      }

      /* ── Topbar ────────────────────────── */
      .mobile-topbar {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        background: #1a1a2e;
        color: #fff;
        padding: 0.7rem 1rem;
        position: sticky;
        top: 0;
        z-index: 100;
        box-shadow: 0 2px 8px rgba(0,0,0,0.15);
      }

      .hamburger {
        background: none;
        border: none;
        color: #fff;
        font-size: 1.4rem;
        cursor: pointer;
        padding: 0.25rem;
        line-height: 1;
        width: 36px;
        height: 36px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 6px;
        transition: background 0.15s;
        flex-shrink: 0;
      }

      .hamburger:hover {
        background: rgba(255,255,255,0.15);
      }

      .hamburger-icon {
        display: block;
        line-height: 1;
      }

      .topbar-title {
        font-weight: 700;
        font-size: 1.05rem;
        flex: 1;
      }

      .topbar-user {
        font-size: 0.8rem;
        color: #aaa;
        max-width: 120px;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      /* ── Backdrop ──────────────────────── */
      .sidebar-open .sidebar-backdrop {
        display: block;
        position: fixed;
        inset: 0;
        background: rgba(0,0,0,0.4);
        z-index: 105;
        animation: fade-in 0.2s ease-out;
      }

      /* ── Sidebar (off-canvas) ─────────── */
      .admin-nav {
        position: fixed;
        top: 0;
        left: 0;
        height: 100vh;
        width: 260px;
        transform: translateX(-100%);
        transition: transform 0.25s ease;
        z-index: 110;
        box-shadow: 4px 0 16px rgba(0,0,0,0.2);
        padding-top: calc(1rem + env(safe-area-inset-top, 0px));
      }

      .sidebar-open .admin-nav {
        transform: translateX(0);
      }

      /* ── Content ──────────────────────── */
      .admin-content {
        padding: 1rem 0.75rem;
        min-height: calc(100vh - 52px);
      }
    }

    /* ═══════════════════════════════════════════
       Very small phones (≤400px)
       ═══════════════════════════════════════════ */
    @media (max-width: 400px) {
      .admin-content {
        padding: 0.75rem 0.5rem;
      }

      .topbar-title {
        font-size: 0.95rem;
      }

      .topbar-user {
        font-size: 0.75rem;
        max-width: 80px;
      }
    }

    @keyframes fade-in {
      from { opacity: 0; }
      to   { opacity: 1; }
    }
  `],
})
export class AdminComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  userName = this.authService.getUserName();
  isSidebarOpen = false;

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSidebar(): void {
    this.isSidebarOpen = false;
  }

  goHome(): void {
    this.closeSidebar();
    this.router.navigate(['/']);
  }

  logout(): void {
    this.closeSidebar();
    this.authService.logout();
  }
}