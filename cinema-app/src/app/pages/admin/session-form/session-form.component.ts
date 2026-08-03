import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe, KeyValuePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../services/admin.service';
import { ToastService } from '../../../services/toast.service';
import { Movie, Room, Session } from '../../../models/cinema.models';

@Component({
  selector: 'app-session-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DatePipe, DecimalPipe, FormsModule, KeyValuePipe],
  template: `
    <div class="page-container">
      <section class="section-card">
        <div style="display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:0.5rem;margin-bottom:1rem">
          <h2 style="margin:0">📅 Sessões Agendadas</h2>
          <button class="btn-replicate" (click)="openReplicateModal()">📋 Replicar Sessões</button>
        </div>
        <div class="filter-bar" style="display:flex;align-items:center;gap:0.75rem;margin-bottom:1.25rem;flex-wrap:wrap">
          <label for="fDate" style="margin:0">Filtrar por data:</label>
          <input id="fDate" type="date" [ngModel]="filterDate" (ngModelChange)="onFilterChange($event)"
            style="width:auto;padding:0.45rem 0.7rem;border:1px solid #ddd;border-radius:8px;font-size:0.85rem" />
          <label for="fStatus" style="margin:0">Status:</label>
          <select id="fStatus" [ngModel]="statusFilter" (ngModelChange)="onStatusChange($event)"
            style="width:auto;padding:0.45rem 0.7rem;border:1px solid #ddd;border-radius:8px;font-size:0.85rem;background:#fff">
            <option value="all">Todas</option>
            <option value="active">Ativas</option>
            <option value="deleted">Desativadas</option>
          </select>
          @if (filterDate || statusFilter !== 'all') { <button (click)="clearFilter()" style="padding:0.35rem 0.7rem;background:#f5f5f5;border:1px solid #ddd;border-radius:6px;cursor:pointer;font-size:0.8rem;color:#666">Limpar</button> }
        </div>

        @if (loadingSessions) { <p class="status-loading">Carregando...</p> }
        @else if (groupedByRoom.size === 0) { <p class="status-empty">Nenhuma sessão agendada{{ filterDate ? ' nesta data' : '' }}.</p> }
        @else {
          @for (entry of groupedByRoom | keyvalue; track entry.key) {
            <div style="margin-bottom:1.5rem">
              <h3 style="color:#1a1a2e;font-size:1rem;margin-bottom:0.5rem;padding-bottom:0.35rem;border-bottom:2px solid #e94560">📍 {{ entry.key }}</h3>
              <div class="table-wrapper">
                <table>
                  <thead><tr><th>Filme</th><th>Início</th><th>Fim</th><th>Preço</th><th>Status</th><th>Ações</th></tr></thead>
                  <tbody>
                    @for (s of entry.value; track s.id) {
                      <tr [class.deleted-row]="s.isDeleted">
                        <td><strong>{{ s.movie?.title }}</strong></td>
                        <td>{{ s.startTime | date:'dd/MM/yyyy HH:mm' }}</td>
                        <td>{{ s.endTime | date:'HH:mm' }}</td>
                        <td>R$ {{ s.ticketPrice | number:'1.2-2' }}</td>
                        <td>
                          @if (s.isDeleted) { <span class="badge badge-deleted">Desativada</span> }
                          @else { <span class="badge badge-active">Ativa</span> }
                        </td>
                        <td>
                          <a [routerLink]="['/admin/sessions', s.id, 'seats']"
                             style="padding:0.25rem 0.6rem;border:none;border-radius:6px;cursor:pointer;background:#e3f2fd;color:#1565c0;font-weight:600;text-decoration:none;font-size:0.85rem;margin-right:0.25rem;display:inline-block">
                            👁 Detalhes
                          </a>
                          @if (s.isDeleted) {
                            <button (click)="restoreSession(s.id)" style="padding:0.25rem 0.6rem;border:none;border-radius:6px;cursor:pointer;background:#e8f5e9;color:#2e7d32;font-weight:600">↩ Restaurar</button>
                          } @else {
                            <button (click)="softDeleteSession(s.id)" style="padding:0.25rem 0.6rem;border:none;border-radius:6px;cursor:pointer;background:#ffebee;color:#c62828;font-weight:600">🗑 Desativar</button>
                          }
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>
          }
        }
        @if (totalPages > 1) {
          <div style="display:flex;align-items:center;justify-content:center;gap:1rem;margin-top:1.5rem;padding-top:1.5rem;border-top:1px solid #eee">
            <button style="padding:0.5rem 1.2rem;background:#1a1a2e;color:#fff;border:none;border-radius:8px;cursor:pointer;font-weight:600;font-size:0.9rem"
              [disabled]="currentPage === 1" (click)="goToPage(currentPage - 1)">
              ← Anterior
            </button>
            <span style="font-size:0.9rem;color:#555">Página {{ currentPage }} de {{ totalPages }} ({{ totalCount }} sessões)</span>
            <button style="padding:0.5rem 1.2rem;background:#1a1a2e;color:#fff;border:none;border-radius:8px;cursor:pointer;font-weight:600;font-size:0.9rem"
              [disabled]="currentPage === totalPages" (click)="goToPage(currentPage + 1)">
              Próxima →
            </button>
          </div>
        }
      </section>

      <section class="section-card max-800">
        <h2>➕ Criar Nova Sessão</h2>
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-row">
            <div class="form-group">
              <label for="movieId">Filme *</label>
              <select id="movieId" formControlName="movieId">
                <option value="" disabled selected>Selecione...</option>
                @for (movie of movies; track movie.id) { <option [value]="movie.id">{{ movie.title }} ({{ movie.durationMinutes }} min)</option> }
              </select>
            </div>
            <div class="form-group">
              <label for="roomId">Sala *</label>
              <select id="roomId" formControlName="roomId">
                <option value="" disabled selected>Selecione...</option>
                @for (room of rooms; track room.id) { <option [value]="room.id">{{ room.name }} ({{ room.rows }}×{{ room.columns }})</option> }
              </select>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group"><label for="start">Início *</label><input id="start" type="datetime-local" formControlName="startTime" /></div>
            <div class="form-group"><label for="end">Fim *</label><input id="end" type="datetime-local" formControlName="endTime" /></div>
            <div class="form-group" style="flex:0.6;min-width:120px"><label for="price">Preço (R$) *</label><input id="price" type="number" formControlName="ticketPrice" placeholder="35.00" min="0" step="0.01" /></div>
          </div>
          @if (successMessage) { <p class="status-success">{{ successMessage }}</p> }
          @if (errorMessage) { <p class="status-error">{{ errorMessage }}</p> }
          <button type="submit" class="btn-primary" [disabled]="form.invalid || loading">
            {{ loading ? 'Salvando...' : 'Criar Sessão' }}
          </button>
        </form>
      </section>

      <!-- ── Modal de Replicação ───────────────────── -->
      @if (showReplicateModal) {
        <div class="modal-backdrop" (click)="closeReplicateModal()">
          <div class="modal-dialog" (click)="$event.stopPropagation()">
            <h2>📋 Replicar Sessões</h2>
            <p class="modal-desc">Copia todas as sessões ativas de um dia para outro, mantendo filmes, salas e horários.</p>

            <div class="form-row">
              <div class="form-group">
                <label for="repSource">Dia de origem</label>
                <input id="repSource" type="date" [(ngModel)]="replicateSourceDate" />
              </div>
              <div class="form-group">
                <label for="repTarget">Dia de destino *</label>
                <input id="repTarget" type="date" [(ngModel)]="replicateTargetDate" />
              </div>
            </div>

            @if (replicateError) {
              <p class="status-error">{{ replicateError }}</p>
            }
            @if (replicateSuccess) {
              <p class="status-success">{{ replicateSuccess }}</p>
            }

            <div class="modal-actions">
              <button class="btn-secondary" (click)="closeReplicateModal()" [disabled]="replicating">Cancelar</button>
              <button class="btn-primary" (click)="onReplicateSubmit()"
                [disabled]="!replicateTargetDate || replicating">
                {{ replicating ? 'Replicando...' : 'Replicar' }}
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class SessionFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminService = inject(AdminService);
  private toast = inject(ToastService);

  movies: Movie[] = [];
  rooms: Room[] = [];
  sessions: Session[] = [];
  groupedByRoom = new Map<string, Session[]>();
  loadingSessions = true;
  filterDate = '';
  statusFilter: 'all' | 'active' | 'deleted' = 'active';

  // Pagination
  currentPage = 1;
  totalPages = 1;
  totalCount = 0;
  readonly pageSize = 20;

  form = this.fb.group({
    movieId: [null as number | null, Validators.required],
    roomId: [null as number | null, Validators.required],
    startTime: ['', Validators.required],
    endTime: ['', Validators.required],
    ticketPrice: [35.00, [Validators.required, Validators.min(0)]]
  });

  loading = false;
  successMessage = '';
  errorMessage = '';

  // ── Replicate Modal ─────────────────────────────
  showReplicateModal = false;
  replicateSourceDate = '';
  replicateTargetDate = '';
  replicating = false;
  replicateError = '';
  replicateSuccess = '';

  ngOnInit(): void {
    this.filterDate = this.formatLocalDate(new Date());
    this.loadSessions();
    this.adminService.getMovies().subscribe({ next: (d) => { this.movies = d; } });
    this.adminService.getRooms().subscribe({ next: (d) => { this.rooms = d; } });
  }

  private formatLocalDate(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  onFilterChange(date: string): void { this.filterDate = date; this.currentPage = 1; this.loadSessions(); }
  onStatusChange(status: string): void { this.statusFilter = status as 'all' | 'active' | 'deleted'; this.currentPage = 1; this.loadSessions(); }
  clearFilter(): void { this.filterDate = ''; this.statusFilter = 'all'; this.currentPage = 1; this.loadSessions(); }

  private loadSessions(): void {
    this.loadingSessions = true;
    this.adminService.getSessionsAdmin(
      this.filterDate || undefined,
      this.currentPage,
      this.pageSize
    ).subscribe({
      next: (data) => {
        const filtered = this.applyStatusFilter(data.items);
        this.totalPages = data.totalPages;
        this.totalCount = data.totalCount;
        this.groupByRoom(filtered);
        this.loadingSessions = false;
      },
      error: () => { this.loadingSessions = false; }
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
    this.loadSessions();
  }

  private applyStatusFilter(sessions: Session[]): Session[] {
    if (this.statusFilter === 'active') return sessions.filter(s => !s.isDeleted);
    if (this.statusFilter === 'deleted') return sessions.filter(s => s.isDeleted);
    return sessions; // 'all'
  }

  private groupByRoom(sessions: Session[]): void {
    const map = new Map<string, Session[]>();
    for (const s of sessions) {
      const name = s.room?.name ?? `Sala ${s.roomId}`;
      if (!map.has(name)) map.set(name, []);
      map.get(name)!.push(s);
    }
    this.groupedByRoom = map;
  }

  async softDeleteSession(id: number): Promise<void> {
    const ok = await this.toast.confirm({
      title: 'Desativar sessão',
      message: 'Esta sessão não aparecerá mais para os usuários.',
      confirmLabel: 'Desativar'
    });
    if (!ok) return;
    this.adminService.softDeleteSession(id).subscribe({
      next: () => { this.toast.success('Sessão desativada.'); this.loadSessions(); },
      error: (err) => { this.toast.error(err.error?.error || 'Erro ao desativar.'); }
    });
  }

  async restoreSession(id: number): Promise<void> {
    const ok = await this.toast.confirm({
      title: 'Restaurar sessão',
      message: 'A sessão voltará a ficar visível para os usuários.',
      confirmLabel: 'Restaurar'
    });
    if (!ok) return;
    this.adminService.restoreSession(id).subscribe({
      next: () => { this.toast.success('Sessão restaurada.'); this.loadSessions(); },
      error: (err) => { this.toast.error(err.error?.error || 'Erro ao restaurar.'); }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.adminService.createSession({
      movieId: this.form.value.movieId!,
      roomId: this.form.value.roomId!,
      startTime: new Date(this.form.value.startTime!).toISOString(),
      endTime: new Date(this.form.value.endTime!).toISOString(),
      ticketPrice: this.form.value.ticketPrice!
    }).subscribe({
      next: (session) => {
        this.loading = false;
        this.successMessage = `Sessão #${session.id} criada!`;
        this.form.reset({ movieId: null, roomId: null, startTime: '', endTime: '', ticketPrice: 35.00 });
        this.loadSessions();
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.error || 'Erro ao criar sessão.';
      }
    });
  }

  // ── Replicate ──────────────────────────────────

  openReplicateModal(): void {
    this.replicateSourceDate = this.filterDate || this.formatLocalDate(new Date());
    this.replicateTargetDate = '';
    this.replicateError = '';
    this.replicateSuccess = '';
    this.showReplicateModal = true;
  }

  closeReplicateModal(): void {
    this.showReplicateModal = false;
    this.replicateTargetDate = '';
    this.replicateError = '';
    this.replicateSuccess = '';
  }

  onReplicateSubmit(): void {
    if (!this.replicateTargetDate || this.replicating) return;

    this.replicating = true;
    this.replicateError = '';
    this.replicateSuccess = '';

    this.adminService.replicateSessions({
      sourceDate: this.replicateSourceDate,
      targetDate: this.replicateTargetDate
    }).subscribe({
      next: (result) => {
        this.replicating = false;
        if (result.createdCount > 0) {
          this.replicateSuccess = `${result.createdCount} sessão(ões) criada(s) com sucesso!`;
        }
        if (result.skippedCount > 0) {
          this.replicateError = `${result.skippedCount} pulada(s): ${result.errors.join('; ')}`;
        }
        if (result.createdCount === 0 && result.skippedCount === 0) {
          this.replicateError = 'Nenhuma sessão ativa encontrada no dia de origem.';
        }
        // Se o destino for a data atual do filtro, recarrega
        if (this.replicateTargetDate === this.filterDate) {
          this.loadSessions();
        }
        // Fecha o modal após 2s se sucesso
        if (result.createdCount > 0 && result.skippedCount === 0) {
          setTimeout(() => this.closeReplicateModal(), 2000);
        }
      },
      error: (err) => {
        this.replicating = false;
        this.replicateError = err.error?.error || 'Erro ao replicar sessões.';
      }
    });
  }
}
