import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminService } from '../../../services/admin.service';
import { ToastService } from '../../../services/toast.service';
import { Movie } from '../../../models/cinema.models';

@Component({
  selector: 'app-movie-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="page-container">
      <!-- ── Lista de filmes ─────────────────────── -->
      <section class="section-card">
        <h2>🎥 Filmes Cadastrados</h2>
        @if (listError) { <p class="status-error">{{ listError }}</p> }
        @if (loadingList) { <p class="status-loading">Carregando...</p> }
        @else if (movies.length === 0) { <p class="status-empty">Nenhum filme cadastrado ainda.</p> }
        @else {
          <div class="table-wrapper">
            <table>
              <thead>
                <tr><th>ID</th><th>Título</th><th>Gênero</th><th>Duração</th><th>Poster</th><th>Ações</th></tr>
              </thead>
              <tbody>
                @for (movie of movies; track movie.id) {
                  <tr>
                    <td>{{ movie.id }}</td>
                    <td><strong>{{ movie.title }}</strong></td>
                    <td>{{ movie.genre }}</td>
                    <td>{{ movie.durationMinutes }} min</td>
                    <td>@if (movie.posterUrl) { <a [href]="movie.posterUrl" target="_blank">🔗</a> } @else { — }</td>
                    <td><button class="btn-danger" (click)="deleteMovie(movie)">🗑</button></td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </section>

      <!-- ── Formulário ─────────────────────────── -->
      <section class="section-card max-800">
        <h2>➕ Cadastrar Novo Filme</h2>
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-row">
            <div class="form-group flex-2">
              <label for="title">Título *</label>
              <input id="title" formControlName="title" placeholder="Ex: Matrix" />
            </div>
            <div class="form-group flex-1">
              <label for="duration">Duração (min) *</label>
              <input id="duration" type="number" formControlName="durationMinutes" placeholder="120" min="1" />
            </div>
          </div>
          <div class="form-group">
            <label for="genre">Gênero</label>
            <input id="genre" formControlName="genre" placeholder="Ex: Ação, Drama" />
          </div>
          <div class="form-group">
            <label for="description">Descrição</label>
            <textarea id="description" formControlName="description" rows="3" placeholder="Sinopse..."></textarea>
          </div>
          <div class="form-group">
            <label for="posterUrl">URL do Poster</label>
            <input id="posterUrl" type="url" formControlName="posterUrl" placeholder="https://..." />
          </div>
          @if (successMessage) { <p class="status-success">{{ successMessage }}</p> }
          @if (errorMessage) { <p class="status-error">{{ errorMessage }}</p> }
          <button type="submit" class="btn-primary" [disabled]="form.invalid || loading">
            {{ loading ? 'Salvando...' : 'Cadastrar Filme' }}
          </button>
        </form>
      </section>
    </div>
  `,
})
export class MovieFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminService = inject(AdminService);
  private toast = inject(ToastService);

  movies: Movie[] = [];
  loadingList = true;
  listError = '';

  form = this.fb.group({
    title: ['', Validators.required],
    description: [''],
    genre: [''],
    durationMinutes: [120, [Validators.required, Validators.min(1)]],
    posterUrl: ['']
  });

  loading = false;
  successMessage = '';
  errorMessage = '';

  ngOnInit(): void { this.loadMovies(); }

  private loadMovies(): void {
    this.loadingList = true;
    this.listError = '';
    this.adminService.getMovies().subscribe({
      next: (data) => { this.movies = data; this.loadingList = false; },
      error: () => { this.loadingList = false; this.listError = 'Erro ao carregar filmes.'; }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.adminService.createMovie({
      title: this.form.value.title!,
      description: this.form.value.description ?? '',
      genre: this.form.value.genre ?? '',
      durationMinutes: this.form.value.durationMinutes!,
      posterUrl: this.form.value.posterUrl ?? ''
    }).subscribe({
      next: (movie) => {
        this.loading = false;
        this.successMessage = `Filme "${movie.title}" cadastrado!`;
        this.form.reset({ title: '', description: '', genre: '', durationMinutes: 120, posterUrl: '' });
        this.loadMovies();
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Erro ao cadastrar filme.';
      }
    });
  }

  async deleteMovie(movie: Movie): Promise<void> {
    const ok = await this.toast.confirm({
      title: 'Excluir filme',
      message: `Excluir PERMANENTEMENTE "${movie.title}" e todas as suas sessões?`,
      confirmLabel: 'Excluir'
    });
    if (!ok) return;

    this.adminService.deleteMovie(movie.id).subscribe({
      next: () => {
        this.toast.success(`Filme "${movie.title}" excluído.`);
        this.loadMovies();
      },
      error: (err) => {
        this.toast.error(err.error?.error || 'Erro ao excluir filme.');
      }
    });
  }
}
