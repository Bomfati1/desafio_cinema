import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { AdminService } from '../../../services/admin.service';
import { CinemaService } from '../../../services/cinema.service';
import { AdminSeat, SessionDetail } from '../../../models/cinema.models';

@Component({
  selector: 'app-admin-session-seats',
  standalone: true,
  imports: [RouterLink, DatePipe, DecimalPipe],
  template: `
    <div class="seats-container">
      <a routerLink="/admin/sessions" class="back-link">← Voltar para sessões</a>

      @if (loading) {
        <p class="status-loading">Carregando mapa de assentos...</p>
      } @else if (error) {
        <p class="status-error">{{ error }}</p>
      } @else {
        <!-- Session Info -->
        <div class="session-header">
          <h1>
            @if (session) {
              {{ session.movie.title }} — {{ session.room.name }}
            } @else {
              Sessão #{{ sessionId }}
            }
          </h1>
          @if (session) {
            <p class="meta">
              {{ session.startTime | date:'dd/MM/yyyy' }} •
              {{ session.startTime | date:'HH:mm' }} - {{ session.endTime | date:'HH:mm' }} •
              R$ {{ session.ticketPrice | number:'1.2-2' }}
            </p>
          }
        </div>

        <!-- Screen -->
        <div class="screen">TELA</div>

        <!-- Seat Grid -->
        <div class="seat-grid" [style.grid-template-columns]="'repeat(' + columns + ', 1fr)'">
          @for (seat of seats; track seat.id) {
            <button
              class="seat"
              [class.occupied]="seat.isOccupied"
              [class.free]="!seat.isOccupied"
              (click)="onSeatClick(seat)"
              [title]="seat.label">
              {{ seat.label }}
            </button>
          }
        </div>

        <!-- Legend -->
        <div class="legend">
          <span class="legend-item"><span class="dot free"></span> Livre</span>
          <span class="legend-item"><span class="dot occupied"></span> Ocupado</span>
        </div>

        <!-- Stats -->
        <div class="stats">
          <p><strong>{{ occupiedCount }}</strong> de <strong>{{ seats.length }}</strong> assentos ocupados</p>
        </div>
      }

      <!-- Popup Modal -->
      @if (selectedSeat) {
        <div class="modal-backdrop" (click)="closePopup()">
          <div class="modal-dialog" (click)="$event.stopPropagation()">
            <h3>🔍 Detalhes da Reserva</h3>
            <div class="modal-body">
              <div class="info-row">
                <span class="info-label">Assento:</span>
                <span class="info-value">{{ selectedSeat.label }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">Cliente:</span>
                <span class="info-value">{{ selectedSeat.customerName }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">Data/Hora:</span>
                <span class="info-value">
                  {{ selectedSeat.reservedAt | date:'dd/MM/yyyy HH:mm' }}
                </span>
              </div>
            </div>
            <div class="modal-actions">
              <button class="btn-primary" (click)="closePopup()">Fechar</button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .seats-container {
      max-width: 700px;
      margin: 0 auto;
      padding: 1rem;
      font-family: 'Segoe UI', system-ui, sans-serif;
    }

    .back-link {
      color: #4a4aff;
      text-decoration: none;
      font-size: 0.9rem;
      display: inline-block;
      margin-bottom: 1rem;
    }

    .back-link:hover {
      text-decoration: underline;
    }

    .session-header {
      text-align: center;
      margin: 1rem 0;
    }

    .session-header h1 {
      margin: 0;
      color: #1a1a2e;
      font-size: 1.3rem;
    }

    .meta {
      color: #666;
      font-size: 0.9rem;
      margin-top: 0.25rem;
    }

    .screen {
      text-align: center;
      background: #ddd;
      color: #666;
      padding: 0.5rem;
      margin: 1.5rem auto;
      width: 60%;
      border-radius: 4px;
      font-weight: bold;
      letter-spacing: 4px;
    }

    .seat-grid {
      display: grid;
      gap: 8px;
      max-width: 400px;
      margin: 0 auto;
    }

    .seat {
      aspect-ratio: 1;
      border: 2px solid #4caf50;
      background: #e8f5e9;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.75rem;
      font-weight: bold;
      color: #2e7d32;
      transition: all 0.15s ease;
    }

    .seat.free:hover {
      background: #c8e6c9;
    }

    .seat.occupied {
      border-color: #e94560;
      background: #fce4ec;
      color: #c62828;
      cursor: pointer;
    }

    .seat.occupied:hover {
      background: #f8bbd0;
      transform: scale(1.08);
    }

    .legend {
      display: flex;
      gap: 1.5rem;
      justify-content: center;
      margin: 1rem 0;
      font-size: 0.85rem;
      color: #555;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 0.35rem;
    }

    .dot {
      width: 14px;
      height: 14px;
      border-radius: 3px;
      display: inline-block;
    }

    .dot.free { background: #e8f5e9; border: 2px solid #4caf50; }
    .dot.occupied { background: #fce4ec; border: 2px solid #e94560; }

    .stats {
      text-align: center;
      margin-top: 1rem;
      color: #555;
      font-size: 0.9rem;
    }

    .status-loading, .status-error {
      text-align: center;
      padding: 3rem;
      color: #666;
      font-size: 1.1rem;
    }

    .status-error { color: #d32f2f; }

    /* ── Modal overrides ── */
    .modal-dialog {
      max-width: 380px;
      width: 90%;
    }

    .modal-dialog h3 {
      margin: 0 0 1rem 0;
      color: #1a1a2e;
      font-size: 1.1rem;
    }

    .modal-body {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      margin-bottom: 1.5rem;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.5rem 0.75rem;
      background: #f5f5f5;
      border-radius: 8px;
    }

    .info-label {
      font-weight: 600;
      color: #555;
      font-size: 0.9rem;
    }

    .info-value {
      color: #1a1a2e;
      font-size: 0.95rem;
      text-align: right;
    }

    .modal-actions {
      display: flex;
      justify-content: flex-end;
    }

    .btn-primary {
      padding: 0.5rem 1.5rem;
      background: #e94560;
      color: #fff;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-size: 0.95rem;
      font-weight: 600;
    }

    .btn-primary:hover {
      background: #c62828;
    }
  `]
})
export class AdminSessionSeatsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private adminService = inject(AdminService);
  private cinemaService = inject(CinemaService);

  sessionId = 0;
  session: SessionDetail | null = null;
  seats: AdminSeat[] = [];
  columns = 4; // fallback
  loading = true;
  error = '';
  selectedSeat: AdminSeat | null = null;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error = 'Sessão não encontrada.';
      this.loading = false;
      return;
    }
    this.sessionId = id;
    this.loadData();
  }

  private loadData(): void {
    this.loading = true;

    // Busca assentos com dados de reserva (admin endpoint)
    this.adminService.getSessionSeats(this.sessionId).subscribe({
      next: (data) => {
        this.seats = data;
        this.loading = false;
        // Calcula colunas com base nos assentos
        if (data.length > 0) {
          this.columns = Math.max(...data.map(s => s.number));
        }
      },
      error: (err) => {
        this.error = err.error?.error || 'Erro ao carregar assentos.';
        this.loading = false;
      }
    });

    // Busca dados da sessão (filme, sala, horário) — pode falhar se deletada
    this.cinemaService.getSessionById(this.sessionId).subscribe({
      next: (data) => { this.session = data; },
      error: () => { /* sessão pode estar deletada — ignorar silenciosamente */ }
    });
  }

  get occupiedCount(): number {
    return this.seats.filter(s => s.isOccupied).length;
  }

  onSeatClick(seat: AdminSeat): void {
    if (seat.isOccupied) {
      this.selectedSeat = seat;
    }
  }

  closePopup(): void {
    this.selectedSeat = null;
  }
}
