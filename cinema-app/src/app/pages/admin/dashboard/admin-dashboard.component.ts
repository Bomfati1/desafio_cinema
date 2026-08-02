import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="dashboard">
      <h1>Dashboard Admin</h1>
      <p>Bem-vindo, <strong>{{ userName }}</strong>!</p>

      <div class="cards">
        <a class="card" routerLink="/admin/movies">
          <h3>🎥 Gerenciar Filmes</h3>
          <p>Cadastre novos filmes no catálogo</p>
        </a>
        <a class="card" routerLink="/admin/rooms">
          <h3>🏠 Gerenciar Salas</h3>
          <p>Crie novas salas com assentos automáticos</p>
        </a>
        <a class="card" routerLink="/admin/sessions">
          <h3>📅 Nova Sessão</h3>
          <p>Crie sessões (Filme + Sala + Horário + Preço)</p>
        </a>
      </div>
    </div>
  `,
  styles: [`
    .dashboard {
      font-family: 'Segoe UI', system-ui, sans-serif;
    }

    h1 {
      color: #1a1a2e;
      margin: 0 0 0.5rem 0;
    }

    p {
      color: #666;
      margin: 0 0 2rem 0;
    }

    .cards {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 1.5rem;
    }

    .card {
      background: #fff;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06);
      cursor: pointer;
      transition: transform 0.15s, box-shadow 0.15s;
      border: 1px solid #e0e0e0;
      text-decoration: none;
      display: block;
    }

    .card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 16px rgba(0,0,0,0.1);
      border-color: #e94560;
    }

    .card h3 {
      margin: 0 0 0.5rem 0;
      color: #16213e;
    }

    .card p {
      margin: 0;
      font-size: 0.9rem;
      color: #888;
    }
  `]
})
export class AdminDashboardComponent {
  private authService = inject(AuthService);

  userName = this.authService.getUserName();
}
