import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="login-container">
      <div class="login-card">
        <h1>🎬 Cinema Login</h1>
        <p class="subtitle">Faça login para continuar</p>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="email">Email</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              placeholder="seu@email.com"
              autocomplete="email"
            />
          </div>

          <div class="form-group">
            <label for="password">Senha</label>
            <input
              id="password"
              type="password"
              formControlName="password"
              placeholder="Sua senha"
              autocomplete="current-password"
            />
          </div>

          @if (errorMessage) {
            <p class="error">{{ errorMessage }}</p>
          }

          <button
            type="submit"
            class="btn-login"
            [disabled]="loginForm.invalid || loading"
          >
            {{ loading ? 'Entrando...' : 'Entrar' }}
          </button>
        </form>

        <div class="credentials-hint">
          <p><strong>Credenciais de teste:</strong></p>
          <p>Admin: <code>admin&#64;cinema.com</code> / <code>admin</code></p>
          <p>User: <code>user&#64;email.com</code> / <code>user</code></p>
        </div>

        <a routerLink="/" class="back-link">← Voltar para sessões</a>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f0f2f5;
      font-family: 'Segoe UI', system-ui, sans-serif;
      padding: 1rem;
    }

    .login-card {
      background: #fff;
      border-radius: 12px;
      padding: 2.5rem;
      width: 100%;
      max-width: 420px;
      box-shadow: 0 4px 24px rgba(0,0,0,0.08);
    }

    h1 {
      text-align: center;
      color: #1a1a2e;
      margin: 0 0 0.25rem 0;
    }

    .subtitle {
      text-align: center;
      color: #666;
      margin: 0 0 2rem 0;
    }

    .form-group {
      margin-bottom: 1.25rem;
    }

    label {
      display: block;
      margin-bottom: 0.4rem;
      font-weight: 600;
      color: #333;
      font-size: 0.9rem;
    }

    input {
      width: 100%;
      padding: 0.7rem 0.8rem;
      border: 1px solid #ddd;
      border-radius: 8px;
      font-size: 1rem;
      box-sizing: border-box;
      transition: border-color 0.2s;
    }

    input:focus {
      outline: none;
      border-color: #1a1a2e;
      box-shadow: 0 0 0 3px rgba(26,26,46,0.1);
    }

    input.ng-invalid.ng-touched {
      border-color: #d32f2f;
    }

    .btn-login {
      width: 100%;
      padding: 0.75rem;
      background: #1a1a2e;
      color: #fff;
      border: none;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      margin-top: 0.5rem;
      transition: background 0.2s;
    }

    .btn-login:hover:not(:disabled) {
      background: #e94560;
    }

    .btn-login:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .error {
      color: #d32f2f;
      font-size: 0.9rem;
      text-align: center;
      margin: 0.5rem 0;
    }

    .credentials-hint {
      margin-top: 2rem;
      padding: 1rem;
      background: #f8f9fa;
      border-radius: 8px;
      font-size: 0.85rem;
      color: #555;
      border: 1px dashed #ddd;
    }

    .credentials-hint p {
      margin: 0.2rem 0;
    }

    .credentials-hint code {
      background: #e8e8e8;
      padding: 0.1rem 0.3rem;
      border-radius: 4px;
      font-size: 0.85rem;
    }

    .back-link {
      display: block;
      text-align: center;
      margin-top: 1.5rem;
      color: #666;
      text-decoration: none;
      font-size: 0.9rem;
    }

    .back-link:hover {
      color: #1a1a2e;
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  loading = false;
  errorMessage = '';

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.loading = true;
    this.errorMessage = '';

    const { email, password } = this.loginForm.value;

    this.authService.login(email!, password!).subscribe({
      next: () => {
        this.loading = false;
        // Redireciona baseado no perfil
        if (this.authService.isAdmin()) {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.loading = false;
        if (err.status === 401) {
          this.errorMessage = 'Email ou senha inválidos.';
        } else {
          this.errorMessage = 'Erro ao conectar ao servidor. Verifique se o backend está rodando.';
        }
      }
    });
  }
}
