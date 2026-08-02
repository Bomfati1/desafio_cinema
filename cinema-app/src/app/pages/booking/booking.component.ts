import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';
import { CinemaService } from '../../services/cinema.service';
import { Seat, SessionDetail } from '../../models/cinema.models';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DatePipe, DecimalPipe],
  template: `
    <div class="booking-container">
      <a routerLink="/" class="back-link">← Voltar para sessões</a>

      @if (loading) {
        <p class="loading">Carregando mapa de assentos...</p>
      } @else if (error) {
        <p class="error">{{ error }}</p>
      } @else if (session) {
        <div class="movie-header">
          @if (session.movie.posterUrl) {
            <img
              class="poster"
              [src]="session.movie.posterUrl"
              [alt]="'Poster: ' + session.movie.title"
              (error)="onPosterError($event)"
            />
          } @else {
            <div class="poster-placeholder">🎬</div>
          }
          <h1>{{ session.movie.title }}</h1>
          <p class="meta">
            {{ session.startTime | date:'dd/MM/yyyy' }} •
            {{ session.startTime | date:'HH:mm' }} - {{ session.endTime | date:'HH:mm' }} •
            {{ session.room.name }}
          </p>
        </div>

        <!-- Screen -->
        <div class="screen">TELA</div>

        <!-- Seat Grid -->
        <div class="seat-grid" [style.grid-template-columns]="'repeat(' + session.room.columns + ', 1fr)'">
          @for (seat of seats; track seat.id) {
            <button
              class="seat"
              [class.occupied]="seat.isOccupied"
              [class.selected]="isSelected(seat)"
              [disabled]="seat.isOccupied"
              (click)="toggleSeat(seat)"
              [title]="seat.label">
              {{ seat.label }}
            </button>
          }
        </div>

        <!-- Legend -->
        <div class="legend">
          <span class="legend-item"><span class="dot free"></span> Livre</span>
          <span class="legend-item"><span class="dot selected"></span> Selecionado</span>
          <span class="legend-item"><span class="dot occupied"></span> Ocupado</span>
        </div>

        <!-- Booking Form -->
        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="booking-form">
          <div class="form-row">
            <label for="name">Seu nome:</label>
            <input id="name" type="text" formControlName="customerName"
                   placeholder="Digite seu nome completo" />
            @if (form.get('customerName')?.touched && form.get('customerName')?.invalid) {
              <span class="field-error">Nome é obrigatório.</span>
            }
          </div>

          <div class="selected-info">
            <p><strong>Assentos:</strong> {{ selectedLabelList || 'Nenhum selecionado' }}</p>
            <p><strong>Total:</strong> R$ {{ totalPrice | number:'1.2-2' }}</p>
          </div>

          @if (submitError) {
            <p class="submit-error">{{ submitError }}</p>
          }
          @if (submitSuccess) {
            <p class="submit-success">{{ submitSuccess }}</p>
          }

          <button type="submit" class="btn-confirm"
                  [disabled]="form.invalid || selectedSeats.size === 0 || submitting">
            {{ submitting ? 'Reservando...' : 'Confirmar Reserva' }}
          </button>
        </form>
      }
    </div>
  `,
  styles: [`
    .booking-container {
      max-width: 700px;
      margin: 0 auto;
      padding: 2rem 1rem;
      font-family: 'Segoe UI', system-ui, sans-serif;
    }

    .back-link {
      color: #4a4aff;
      text-decoration: none;
      font-size: 0.9rem;
    }

    .movie-header {
      text-align: center;
      margin: 1.5rem 0;
    }

    .poster {
      width: 180px;
      height: 270px;
      object-fit: cover;
      border-radius: 10px;
      margin: 0 auto 1rem;
      display: block;
      box-shadow: 0 4px 16px rgba(0,0,0,0.2);
      background: #f0f0f0;
    }

    .poster-placeholder {
      width: 180px;
      height: 270px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f0f0f0;
      border-radius: 10px;
      margin: 0 auto 1rem;
      font-size: 3rem;
      color: #999;
    }

    .movie-header h1 {
      margin: 0;
      color: #1a1a2e;
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

    .seat:hover:not(.occupied) {
      background: #c8e6c9;
    }

    .seat.occupied {
      border-color: #ccc;
      background: #f5f5f5;
      color: #bbb;
      cursor: not-allowed;
    }

    .seat.selected {
      border-color: #e94560;
      background: #fce4ec;
      color: #c62828;
    }

    .legend {
      display: flex;
      gap: 1.5rem;
      justify-content: center;
      margin: 1rem 0 2rem;
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
    .dot.selected { background: #fce4ec; border: 2px solid #e94560; }
    .dot.occupied { background: #f5f5f5; border: 2px solid #ccc; }

    .booking-form {
      background: #fafafa;
      border: 1px solid #e0e0e0;
      border-radius: 12px;
      padding: 1.5rem;
    }

    .form-row {
      display: flex;
      flex-direction: column;
      gap: 0.3rem;
      margin-bottom: 1rem;
    }

    .form-row label {
      font-weight: 600;
      color: #333;
    }

    .form-row input {
      padding: 0.6rem;
      border: 1px solid #ccc;
      border-radius: 6px;
      font-size: 1rem;
    }

    .field-error {
      color: #d32f2f;
      font-size: 0.8rem;
    }

    .selected-info {
      margin-bottom: 1rem;
    }

    .selected-info p {
      margin: 0.2rem 0;
    }

    .btn-confirm {
      width: 100%;
      padding: 0.75rem;
      background: #e94560;
      color: #fff;
      border: none;
      border-radius: 8px;
      font-size: 1.1rem;
      font-weight: bold;
      cursor: pointer;
      transition: background 0.2s;
    }

    .btn-confirm:hover:not(:disabled) {
      background: #c62828;
    }

    .btn-confirm:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .submit-error {
      color: #d32f2f;
      font-weight: 600;
      margin: 0.5rem 0;
    }

    .submit-success {
      color: #2e7d32;
      font-weight: 600;
      margin: 0.5rem 0;
    }

    .loading, .error {
      text-align: center;
      padding: 3rem;
      color: #666;
      font-size: 1.1rem;
    }

    .error { color: #d32f2f; }
  `]
})
export class BookingComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private cinemaService = inject(CinemaService);
  private fb = inject(FormBuilder);

  session: SessionDetail | null = null;
  seats: Seat[] = [];
  selectedSeats = new Set<number>();
  loading = true;
  submitting = false;
  error = '';
  submitError = '';
  submitSuccess = '';

  form = this.fb.group({
    customerName: ['', Validators.required]
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error = 'Sessão não encontrada.';
      this.loading = false;
      return;
    }

    this.cinemaService.getSessionById(id).subscribe({
      next: (data) => {
        this.session = data;
        this.seats = data.seats;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar dados da sessão.';
        this.loading = false;
        console.error('API error:', err);
      }
    });
  }

  isSelected(seat: Seat): boolean {
    return this.selectedSeats.has(seat.id);
  }

  toggleSeat(seat: Seat): void {
    if (seat.isOccupied) return;
    if (this.selectedSeats.has(seat.id)) {
      this.selectedSeats.delete(seat.id);
    } else {
      this.selectedSeats.add(seat.id);
    }
  }

  get selectedLabelList(): string {
    return [...this.selectedSeats]
      .map(id => this.seats.find(s => s.id === id)?.label)
      .filter(Boolean)
      .join(', ');
  }

  get totalPrice(): number {
    return this.selectedSeats.size * (this.session?.ticketPrice ?? 0);
  }

  onSubmit(): void {
    if (this.form.invalid || this.selectedSeats.size === 0 || !this.session) return;

    this.submitting = true;
    this.submitError = '';
    this.submitSuccess = '';

    this.cinemaService.createReservation({
      sessionId: this.session.id,
      customerName: this.form.value.customerName!,
      seatIds: [...this.selectedSeats]
    }).subscribe({
      next: () => {
        this.submitSuccess = '✅ Reserva confirmada com sucesso!';
        this.selectedSeats.clear();
        this.form.reset();
        this.submitting = false;

        // Recarrega os assentos para refletir ocupação
        this.loadSessionAgain();
      },
      error: (err) => {
        this.submitError = err?.error?.message
          ?? 'Erro ao confirmar reserva. Os assentos podem já estar ocupados.';
        this.submitting = false;
        this.loadSessionAgain();
        console.error('Reservation error:', err);
      }
    });
  }

  private loadSessionAgain(): void {
    if (!this.session) return;
    this.cinemaService.getSessionById(this.session.id).subscribe({
      next: (data) => {
        this.session = data;
        this.seats = data.seats;
      }
    });
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
