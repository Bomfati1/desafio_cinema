import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe, KeyValuePipe } from '@angular/common';
import { CinemaService } from '../../services/cinema.service';
import { AuthService } from '../../services/auth.service';
import { Session } from '../../models/cinema.models';

@Component({
  selector: 'app-session-list',
  standalone: true,
  imports: [RouterLink, DatePipe, DecimalPipe, KeyValuePipe],
  template: `
    <div class="sessions-container">
      <h1>🎬 Cinema - Sessões Disponíveis</h1>

      <!-- ── Navegação de dias (7 dias) ──────────── -->
      <div class="day-strip-wrapper">
        <div class="day-strip">
          @for (day of days; track day.date) {
            <button
              class="day-chip"
              [class.day-active]="day.date === selectedDate"
              [class.day-today]="day.isToday"
              (click)="onDaySelect(day.date)">
              <span class="day-name">{{ day.label }}</span>
              <span class="day-num">{{ day.day }}</span>
            </button>
          }
        </div>
      </div>

      <!-- ── Filtros de gênero ────────────────────── -->
      <div class="genre-filters">
        <button
          class="genre-chip"
          [class.genre-active]="!selectedGenre"
          (click)="onGenreSelect(null)">
          🎬 Todos
        </button>
        @for (genre of availableGenres; track genre) {
          <button
            class="genre-chip"
            [class.genre-active]="genre === selectedGenre"
            (click)="onGenreSelect(genre)">
            {{ genre }}
          </button>
        }
      </div>

      @if (loading) {
        <p class="loading">Carregando sessões...</p>
      } @else if (error) {
        <p class="error">{{ error }}</p>
      } @else if (groupedByRoom.size === 0) {
        <p class="empty">Nenhuma sessão disponível nesta data.</p>
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
                      🕐 {{ session.startTime | date:'HH:mm' }} - {{ session.endTime | date:'HH:mm' }}
                    </p>
                    <p class="price">💵 R$ {{ session.ticketPrice | number:'1.2-2' }}</p>
                  </div>
                  <button class="btn-synopsis" (click)="openSynopsis(session)">📖 Sinopse</button>
                  @if (!isAdmin) {
                    <a [routerLink]="['/booking', session.id]" class="btn-reserve">
                      Reservar Assentos
                    </a>
                  }
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

      <!-- Synopsis Modal -->
      @if (selectedSynopsis) {
        <div class="synopsis-backdrop" (click)="closeSynopsis()">
          <div class="synopsis-dialog" (click)="$event.stopPropagation()">
            <h2>{{ selectedSynopsis.movie?.title }}</h2>
            @if (selectedSynopsis.movie?.genre) {
              <span class="genre">{{ selectedSynopsis.movie?.genre }}</span>
            }
            @if (selectedSynopsis.movie?.durationMinutes) {
              <span style="color:#666;font-size:0.85rem;margin-left:0.5rem">{{ selectedSynopsis.movie?.durationMinutes }} min</span>
            }
            <hr style="margin:1rem 0;border:none;border-top:1px solid #eee" />
            <p class="synopsis-text">{{ selectedSynopsis.movie?.description || 'Sinopse não disponível.' }}</p>
            <button class="btn-close-synopsis" (click)="closeSynopsis()">Fechar</button>
          </div>
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

    /* ── Day Navigation Strip ──────────── */
    .day-strip-wrapper {
      overflow-x: auto;
      margin-bottom: 1rem;
      scrollbar-width: thin;
      scrollbar-color: #e94560 transparent;
    }

    .day-strip-wrapper::-webkit-scrollbar {
      height: 4px;
    }

    .day-strip-wrapper::-webkit-scrollbar-thumb {
      background: #e94560;
      border-radius: 2px;
    }

    .day-strip {
      display: flex;
      justify-content: center;
      gap: 0.5rem;
      min-width: fit-content;
    }

    .day-chip {
      flex: 0 0 auto;
      min-width: 56px;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.15rem;
      padding: 0.55rem 0.5rem;
      border: 1px solid #e0e0e0;
      border-radius: 10px;
      background: #fff;
      cursor: pointer;
      font-family: inherit;
      transition: all 0.15s ease;
    }

    .day-chip:hover {
      border-color: #e94560;
    }

    .day-chip.day-active {
      background: #e94560;
      border-color: #e94560;
      color: #fff;
    }

    .day-name {
      font-size: 0.72rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.3px;
    }

    .day-num {
      font-size: 1.1rem;
      font-weight: 700;
      line-height: 1;
    }

    .day-today .day-name {
      color: inherit;
    }

    .day-today:not(.day-active) {
      border-color: #e94560;
    }

    .day-today:not(.day-active)::after {
      content: '';
      width: 5px;
      height: 5px;
      border-radius: 50%;
      background: #e94560;
      margin-top: 1px;
    }

    .day-active.day-today::after {
      content: '';
      width: 5px;
      height: 5px;
      border-radius: 50%;
      background: #fff;
      margin-top: 1px;
    }

    /* ── Genre Filter Chips ───────────── */
    .genre-filters {
      display: flex;
      flex-wrap: wrap;
      justify-content: center;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
    }

    .genre-chip {
      padding: 0.4rem 1rem;
      background: #fff;
      border: 1px solid #ddd;
      border-radius: 20px;
      font-size: 0.82rem;
      color: #555;
      cursor: pointer;
      font-family: inherit;
      transition: all 0.15s ease;
    }

    .genre-chip:hover {
      border-color: #e94560;
      color: #e94560;
    }

    .genre-chip.genre-active {
      background: #1a1a2e;
      border-color: #1a1a2e;
      color: #fff;
      font-weight: 600;
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

    .btn-synopsis {
      padding: 0.4rem;
      background: transparent;
      border: 1px solid #ddd;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.8rem;
      color: #555;
      transition: all 0.15s;
    }

    .btn-synopsis:hover {
      background: #f5f0ff;
      border-color: #7c4dff;
      color: #7c4dff;
    }

    .synopsis-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.5);
      z-index: 10000;
      display: flex;
      align-items: center;
      justify-content: center;
      animation: fade-in 0.15s ease-out;
    }

    .synopsis-dialog {
      background: #fff;
      border-radius: 14px;
      padding: 2rem;
      max-width: 520px;
      width: 90%;
      max-height: 80vh;
      overflow-y: auto;
      box-shadow: 0 8px 32px rgba(0,0,0,0.2);
    }

    .synopsis-dialog h2 {
      margin: 0 0 0.5rem 0;
      color: #1a1a2e;
    }

    .synopsis-text {
      color: #444;
      line-height: 1.7;
      font-size: 0.95rem;
    }

    .btn-close-synopsis {
      display: block;
      width: 100%;
      margin-top: 1.5rem;
      padding: 0.6rem;
      background: #1a1a2e;
      color: #fff;
      border: none;
      border-radius: 8px;
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      transition: background 0.2s;
    }

    .btn-close-synopsis:hover { background: #e94560; }

    @keyframes fade-in {
      from { opacity: 0; }
      to   { opacity: 1; }
    }
  `]
})
export class SessionListComponent implements OnInit {
  private cinemaService = inject(CinemaService);
  private authService = inject(AuthService);

  isAdmin = this.authService.isAdmin();
  sessions: Session[] = [];
  groupedByRoom = new Map<string, Session[]>();
  loading = true;
  error = '';
  selectedDate = '';
  selectedGenre: string | null = null;
  selectedSynopsis: Session | null = null;
  days: Array<{ date: string; label: string; day: number; isToday: boolean }> = [];

  // Pagination state
  currentPage = 1;
  totalPages = 1;
  totalCount = 0;
  readonly pageSize = 20;

  get hasPreviousPage(): boolean { return this.currentPage > 1; }
  get hasNextPage(): boolean { return this.currentPage < this.totalPages; }

  ngOnInit(): void {
    this.generateDays();
    this.selectedDate = this.formatLocalDate(new Date());
    this.loadSessions();
  }

  private formatLocalDate(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  private WEEKDAYS = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];

  private generateDays(): void {
    const today = new Date();
    this.days = Array.from({ length: 7 }, (_, i) => {
      const d = new Date(today);
      d.setDate(today.getDate() + i);
      return {
        date: this.formatLocalDate(d),
        label: this.WEEKDAYS[d.getDay()],
        day: d.getDate(),
        isToday: i === 0
      };
    });
  }

  get availableGenres(): string[] {
    const genres = this.sessions
      .map(s => s.movie?.genre)
      .filter((g): g is string => !!g);
    return [...new Set(genres)].sort();
  }

  onDaySelect(date: string): void {
    if (date === this.selectedDate) return;
    this.selectedDate = date;
    this.currentPage = 1;
    this.loadSessions();
  }

  onGenreSelect(genre: string | null): void {
    if (genre === this.selectedGenre) return;
    this.selectedGenre = genre;
    this.currentPage = 1;
    this.applyFiltersAndGroup();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
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
    if (this.selectedGenre) {
      filtered = filtered.filter(s => s.movie?.genre === this.selectedGenre);
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

  openSynopsis(session: Session): void {
    this.selectedSynopsis = session;
  }

  closeSynopsis(): void {
    this.selectedSynopsis = null;
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
