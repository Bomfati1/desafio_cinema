import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe, KeyValuePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CinemaService } from '../../services/cinema.service';
import { Session, PagedResult } from '../../models/cinema.models';

@Component({
  selector: 'app-session-list',
  standalone: true,
  imports: [RouterLink, DatePipe, DecimalPipe, KeyValuePipe, FormsModule],
  template: `
    <div class="sessions-container">
      <h1>🎬 Cinema - Sessões Disponíveis</h1>

      <!-- ── Filtro por data ───────────────────────── -->
      <div class="filter-bar">
        <label for="filterDate">📅 Data:</label>
        <input
          id="filterDate"
          type="date"
          [ngModel]="selectedDate"
          (ngModelChange)="onDateChange($event)"
        />
        <label class="toggle-past">
          <input type="checkbox" [ngModel]="showPast" (ngModelChange)="onShowPastChange($event)" />
          Mostrar sessões passadas
        </label>
        @if (selectedDate) {
          <button class="btn-clear" (click)="clearFilter()">Limpar filtro</button>
        }
      </div>

      @if (loading) {
        <p class="loading">Carregando sessões...</p>
      } @else if (error) {
        <p class="error">{{ error }}</p>
      } @else if (groupedByRoom.size === 0) {
        <p class="empty">Nenhuma sessão disponível{{ selectedDate ? ' nesta data' : '' }}.</p>
      } @else {
        <!-- Agrupado por sala -->
        @for (entry of groupedByRoom | keyvalue; track entry.key) {
          <div class="room-section">
            <h2 class="room-title">📍 {{ entry.key }}</h2>
            <div class="sessions-grid">
              @for (session of entry.value; track session.id) {
                <div class="session-card">
                  <div class="movie-info">
                    @if (session.movie?.posterUrl) {
                      <img
                        class="poster"
                        [src]="session.movie?.posterUrl"
                        [alt]="'Poster: ' + (session.movie?.title ?? '')"
                        (error)="onPosterError($event)"
                      />
                    } @else {
                      <div class="poster-placeholder">🎬</div>
                    }
                    <h3>{{ session.movie?.title }}</h3>
                    <span class="genre">{{ session.movie?.genre }}</span>
                    <p class="duration">{{ session.movie?.durationMinutes }} min</p>
                  </div>
                  <div class="session-info">
                    <p class="time">
                      🕐 {{ session.startTime | date:'dd/MM/yyyy HH:mm' }} - {{ session.endTime | date:'HH:mm' }}
                    </p>
                    <p class="price">💵 R$ {{ session.ticketPrice | number:'1.2-2' }}</p>
                  </div>
                  <a [routerLink]="['/booking', session.id]" class="btn-reserve">
                    Reservar Assentos
                  </a>
                </div>
              }
            </div>
          </div>
        }

        <!-- ── Paginação ─────────────────────────── -->
        <div class="pagination">
          <button
            class="page-btn"
            [disabled]="!hasPreviousPage"
            (click)="goToPage(currentPage - 1)">
            ← Anterior
          </button>

          <span class="page-info">
            Página {{ currentPage }} de {{ totalPages }}
            <span class="total-hint">({{ totalCount }} sessões)</span>
          </span>

          <button
            class="page-btn"
            [disabled]="!hasNextPage"
            (click)="goToPage(currentPage + 1)">
            Próxima →
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .sessions-container {
      max-width: 960px;
      margin: 0 auto;
      padding: 2rem 1rem;
      font-family: 'Segoe UI', system-ui, sans-serif;
    }

    h1 {
      text-align: center;
      margin-bottom: 1.5rem;
      color: #1a1a2e;
    }

    .filter-bar {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.75rem;
      margin-bottom: 2rem;
    }

    .filter-bar label {
      font-weight: 600;
      color: #333;
      font-size: 0.95rem;
    }

    .filter-bar input[type="date"] {
      padding: 0.5rem 0.75rem;
      border: 1px solid #ddd;
      border-radius: 8px;
      font-size: 0.9rem;
      font-family: inherit;
    }

    .filter-bar input:focus {
      outline: none;
      border-color: #1a1a2e;
      box-shadow: 0 0 0 3px rgba(26,26,46,0.1);
    }

    .btn-clear {
      padding: 0.4rem 0.8rem;
      background: #f5f5f5;
      border: 1px solid #ddd;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.85rem;
      color: #666;
      transition: background 0.15s;
    }

    .btn-clear:hover { background: #e0e0e0; }

    .toggle-past {
      display: flex;
      align-items: center;
      gap: 0.35rem;
      font-size: 0.85rem;
      color: #555;
      cursor: pointer;
      user-select: none;
    }

    .toggle-past input[type="checkbox"] {
      cursor: pointer;
      accent-color: #e94560;
    }

    .room-section { margin-bottom: 2.5rem; }

    .room-title {
      color: #1a1a2e;
      margin: 0 0 1rem 0;
      padding-bottom: 0.5rem;
      border-bottom: 2px solid #e94560;
      font-size: 1.1rem;
    }

    .sessions-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 1rem;
    }

    .session-card {
      border: 1px solid #e0e0e0;
      border-radius: 12px;
      padding: 1.25rem;
      background: #fff;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06);
      display: flex;
      flex-direction: column;
      gap: 0.6rem;
      transition: transform 0.15s ease;
    }

    .session-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 16px rgba(0,0,0,0.1);
    }

    .poster {
      width: 100%;
      height: 320px;
      object-fit: cover;
      border-radius: 8px;
      margin-bottom: 0.5rem;
      background: #f0f0f0;
    }

    .poster-placeholder {
      width: 100%;
      height: 320px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f0f0f0;
      border-radius: 8px;
      margin-bottom: 0.5rem;
      font-size: 3rem;
      color: #999;
    }

    .movie-info h3 { margin: 0; font-size: 1.1rem; color: #16213e; }

    .genre {
      display: inline-block;
      background: #e8e8ff;
      color: #4a4aff;
      padding: 0.15rem 0.5rem;
      border-radius: 20px;
      font-size: 0.75rem;
      margin-top: 0.2rem;
    }

    .duration { color: #666; font-size: 0.85rem; margin: 0.2rem 0 0 0; }

    .session-info p { margin: 0.2rem 0; font-size: 0.85rem; color: #444; }

    .price { font-weight: bold; color: #2e7d32 !important; font-size: 1rem !important; }

    .btn-reserve {
      display: block;
      text-align: center;
      background: #1a1a2e;
      color: #fff;
      text-decoration: none;
      padding: 0.55rem;
      border-radius: 8px;
      font-weight: 600;
      font-size: 0.9rem;
      margin-top: auto;
      transition: background 0.2s;
    }

    .btn-reserve:hover { background: #e94560; }

    .loading, .error, .empty {
      text-align: center;
      padding: 3rem;
      color: #666;
      font-size: 1.1rem;
    }

    .error { color: #d32f2f; }

    /* ── Pagination ───────────────────── */
    .pagination {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      margin-top: 2rem;
      padding-top: 1.5rem;
      border-top: 1px solid #eee;
    }

    .page-btn {
      padding: 0.5rem 1.2rem;
      background: #1a1a2e;
      color: #fff;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 600;
      font-size: 0.9rem;
      transition: background 0.15s;
    }

    .page-btn:hover:not(:disabled) { background: #e94560; }

    .page-btn:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .page-info {
      font-size: 0.9rem;
      color: #555;
    }

    .total-hint {
      color: #999;
      font-size: 0.8rem;
    }
  `]
})
export class SessionListComponent implements OnInit {
  private cinemaService = inject(CinemaService);

  sessions: Session[] = [];
  groupedByRoom = new Map<string, Session[]>();
  loading = true;
  error = '';
  selectedDate = '';
  showPast = false;

  // Pagination state
  currentPage = 1;
  totalPages = 1;
  totalCount = 0;
  readonly pageSize = 20;

  get hasPreviousPage(): boolean { return this.currentPage > 1; }
  get hasNextPage(): boolean { return this.currentPage < this.totalPages; }

  ngOnInit(): void {
    this.selectedDate = this.formatLocalDate(new Date());
    this.loadSessions();
  }

  private formatLocalDate(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  onDateChange(date: string): void {
    this.selectedDate = date;
    this.currentPage = 1;
    this.loadSessions();
  }

  onShowPastChange(show: boolean): void {
    this.showPast = show;
    this.applyFiltersAndGroup();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
    this.loadSessions();
  }

  clearFilter(): void {
    this.selectedDate = '';
    this.currentPage = 1;
    this.loadSessions();
  }

  private loadSessions(): void {
    this.loading = true;
    this.error = '';

    this.cinemaService.getSessions({
      date: this.selectedDate || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (data) => {
        this.sessions = data.items;
        this.totalPages = data.totalPages;
        this.totalCount = data.totalCount;
        this.applyFiltersAndGroup();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar sessões. Verifique se o backend está rodando.';
        this.loading = false;
        console.error('API error:', err);
      }
    });
  }

  private applyFiltersAndGroup(): void {
    let filtered = this.sessions;

    if (!this.showPast) {
      const now = new Date();
      filtered = filtered.filter(s => new Date(s.endTime) > now);
    }

    this.groupByRoom(filtered);
  }

  private groupByRoom(sessions: Session[]): void {
    const map = new Map<string, Session[]>();
    for (const s of sessions) {
      const roomName = s.room?.name ?? `Sala ${s.roomId}`;
      if (!map.has(roomName)) {
        map.set(roomName, []);
      }
      map.get(roomName)!.push(s);
    }
    this.groupedByRoom = map;
  }

  onPosterError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
    const placeholder = img.parentElement?.querySelector('.poster-placeholder');
    if (!placeholder) {
      const div = document.createElement('div');
      div.className = 'poster-placeholder';
      div.textContent = '🎬';
      img.insertAdjacentElement('afterend', div);
    } else {
      (placeholder as HTMLElement).style.display = 'flex';
    }
  }
}
