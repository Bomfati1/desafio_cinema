import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminService } from '../../../services/admin.service';
import { ToastService } from '../../../services/toast.service';
import { Room } from '../../../models/cinema.models';

@Component({
  selector: 'app-room-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="page-container">
      <section class="section-card">
        <h2>🏠 Salas Cadastradas</h2>
        @if (listError) { <p class="status-error">{{ listError }}</p> }
        @if (loadingList) { <p class="status-loading">Carregando...</p> }
        @else if (rooms.length === 0) { <p class="status-empty">Nenhuma sala cadastrada ainda.</p> }
        @else {
          <div class="table-wrapper">
            <table>
              <thead><tr><th>Nome</th><th>Fileiras</th><th>Colunas</th><th>Total</th><th>Ações</th></tr></thead>
              <tbody>
                @for (room of rooms; track room.id) {
                  <tr>
                    <td><strong>{{ room.name }}</strong></td>
                    <td>{{ room.rows }}</td><td>{{ room.columns }}</td>
                    <td>{{ room.rows * room.columns }}</td>
                    <td><button class="btn-danger" (click)="deleteRoom(room)">🗑</button></td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </section>

      <section class="section-card max-600">
        <h2>➕ Cadastrar Nova Sala</h2>
        <p style="color:#888;font-size:0.85rem;margin-bottom:1rem">Assentos gerados automaticamente (Fileiras × Colunas).</p>
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="name">Nome *</label>
            <input id="name" formControlName="name" placeholder="Ex: Sala Premium 2" />
          </div>
          <div class="form-row">
            <div class="form-group">
              <label for="rows">Fileiras (A-Z) *</label>
              <input id="rows" type="number" formControlName="rows" placeholder="5" min="1" max="26" />
            </div>
            <div class="form-group">
              <label for="columns">Colunas *</label>
              <input id="columns" type="number" formControlName="columns" placeholder="4" min="1" max="20" />
            </div>
          </div>
          @if (successMessage) { <p class="status-success">{{ successMessage }}</p> }
          @if (errorMessage) { <p class="status-error">{{ errorMessage }}</p> }
          <button type="submit" class="btn-primary" [disabled]="form.invalid || loading">
            {{ loading ? 'Salvando...' : 'Cadastrar Sala' }}
          </button>
        </form>
      </section>
    </div>
  `,
})
export class RoomFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminService = inject(AdminService);
  private toast = inject(ToastService);

  rooms: Room[] = [];
  loadingList = true;
  listError = '';

  form = this.fb.group({
    name: ['', Validators.required],
    rows: [5, [Validators.required, Validators.min(1), Validators.max(26)]],
    columns: [4, [Validators.required, Validators.min(1), Validators.max(20)]]
  });

  loading = false;
  successMessage = '';
  errorMessage = '';

  ngOnInit(): void { this.loadRooms(); }

  private loadRooms(): void {
    this.loadingList = true;
    this.listError = '';
    this.adminService.getRooms().subscribe({
      next: (data) => { this.rooms = data; this.loadingList = false; },
      error: () => { this.loadingList = false; this.listError = 'Erro ao carregar salas.'; }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.adminService.createRoom({
      name: this.form.value.name!,
      rows: this.form.value.rows!,
      columns: this.form.value.columns!
    }).subscribe({
      next: (room) => {
        this.loading = false;
        const total = this.form.value.rows! * this.form.value.columns!;
        this.successMessage = `Sala "${room.name}" cadastrada com ${total} assentos!`;
        this.form.reset({ name: '', rows: 5, columns: 4 });
        this.loadRooms();
      },
      error: () => { this.loading = false; this.errorMessage = 'Erro ao cadastrar sala.'; }
    });
  }

  async deleteRoom(room: Room): Promise<void> {
    const ok = await this.toast.confirm({
      title: 'Excluir sala',
      message: `Excluir PERMANENTEMENTE "${room.name}" e todos os seus assentos/sessões?`,
      confirmLabel: 'Excluir'
    });
    if (!ok) return;

    this.adminService.deleteRoom(room.id).subscribe({
      next: () => {
        this.toast.success(`Sala "${room.name}" excluída.`);
        this.loadRooms();
      },
      error: (err) => {
        this.toast.error(err.error?.error || 'Erro ao excluir sala.');
      }
    });
  }
}
