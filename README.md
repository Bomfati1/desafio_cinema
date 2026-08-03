# 🎬 Cinema App — Sistema de Gestão de Cinema

Aplicação full-stack para gestão de cinema com autenticação JWT (access + refresh tokens), perfis Admin/User, reserva de assentos com proteção contra conflitos, soft-delete de sessões e paginação.

---

## 🛠️ Tecnologias Utilizadas

### Backend (.NET 8 Web API)
| Tecnologia | Versão | Descrição |
|-----------|--------|-----------|
| ASP.NET Core | 8.0 | Web API REST |
| Entity Framework Core | 8.0.14 | ORM + Migrations |
| PostgreSQL (Npgsql) | 8.0.11 | Banco de dados relacional |
| JWT Bearer | 8.0.14 | Autenticação stateless |
| BCrypt.Net-Next | 4.0.3 | Hash de senhas |
| FluentValidation | 11.3.0 | Validação de DTOs |
| Swashbuckle (Swagger) | 6.6.2 | Documentação OpenAPI |
| ProblemDetails (RFC 7807) | — | Padronização de erros |
| Health Checks | — | Monitoramento `/health` |
| CORS | — | Política para Angular local |

### Frontend (Angular 18)
| Tecnologia | Versão | Descrição |
|-----------|--------|-----------|
| Angular | 18.2 | Framework SPA |
| TypeScript | 5.5 | Linguagem tipada |
| RxJS | 7.8 | Programação reativa |
| Standalone Components | — | Arquitetura sem NgModules |
| Reactive Forms | — | Formulários reativos |
| Lazy Loading | — | Carregamento sob demanda |
| Route Guards | — | Proteção de rotas (auth/admin) |
| HTTP Interceptors | — | Injeção de token JWT + refresh automático |

### Banco de Dados
- **PostgreSQL 16+** com 8 tabelas: `Movies`, `Rooms`, `Seats`, `Sessions`, `Reservations`, `Tickets`, `Users`, `RefreshTokens`

---

## 🚀 Como Rodar

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [PostgreSQL 16+](https://www.postgresql.org/download/)

### 1. Configurar Banco e JWT

Crie um arquivo `appsettings.json` na pasta `Cinema.Api/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cinema;Username=postgres;Password=sua_senha"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-com-pelo-menos-32-caracteres",
    "Issuer": "CinemaApi",
    "Audience": "CinemaApp"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

> 💡 **Alternativa (dev):** Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) para não commitar secrets:
> ```bash
> cd Cinema.Api
> dotnet user-secrets init
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=cinema;Username=postgres;Password=sua_senha"
> dotnet user-secrets set "Jwt:Key" "sua-chave-secreta-com-pelo-menos-32-caracteres"
> dotnet user-secrets set "Jwt:Issuer" "CinemaApi"
> dotnet user-secrets set "Jwt:Audience" "CinemaApp"
> ```

### 2. Rodar Backend (.NET 8 Web API)

```bash
cd Cinema.Api

# Restaurar pacotes e rodar
# As migrations são aplicadas automaticamente na inicialização
dotnet run
```

- API: `http://localhost:5125`
- Swagger: `http://localhost:5125/swagger` (abre automaticamente)
- Health check: `http://localhost:5125/health`

### 3. Rodar Frontend (Angular 18)

```bash
cd cinema-app

# Instalar dependências (apenas na primeira vez)
npm install

# Rodar servidor de desenvolvimento
npx ng serve
```

- Frontend: `http://localhost:4200`

---

## 🔐 Autenticação JWT

O sistema implementa **JWT com refresh token rotativo**:
- **Access Token**: curta duração (1h), usado nas requisições autenticadas
- **Refresh Token**: longa duração (7 dias), armazenado no banco, rotacionado a cada uso (cada token só pode ser usado uma vez — `IsRevoked`)
- O frontend detecta `401` e tenta renovar automaticamente via interceptor HTTP

### Usuários Seed

Dois usuários são criados automaticamente na primeira execução:

| Perfil | Email | Senha | Role |
|--------|-------|-------|------|
| **Admin** | `admin@cinema.com` | `admin` | Admin |
| **User** | `user@email.com` | `user` | User |

> Senhas são armazenadas com hash BCrypt (salting automático).

### Permissões por Perfil

| Ação | Admin | User | Público |
|------|:-----:|:----:|:-------:|
| Listar sessões | ✅ | ✅ | ✅ |
| Ver detalhes/mapa de assentos | ✅ | ✅ | ✅ |
| Fazer reserva | ❌ | ✅ | ❌ |
| CRUD filmes | ✅ | ❌ | ❌ |
| CRUD salas | ✅ | ❌ | ❌ |
| Criar/desativar/restaurar sessões | ✅ | ❌ | ❌ |
| Ver assentos com dados de reserva | ✅ | ❌ | ❌ |

---

## 📖 Testando no Swagger

1. Acesse `http://localhost:5125/swagger`
2. Faça `POST /api/auth/login` com:
   ```json
   { "email": "admin@cinema.com", "password": "admin" }
   ```
3. Copie o valor do campo `token` da resposta
4. Clique no botão 🔒 **Authorize** no topo da página
5. Cole **apenas o token** (sem `Bearer `) e clique em "Authorize"
6. Agora você pode testar todas as rotas protegidas!

---

## 📡 Endpoints da API

### 🔓 Autenticação (público)

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/auth/login` | Login — retorna JWT + refresh token |
| `POST` | `/api/auth/refresh` | Renova JWT usando refresh token (rotação) |

**POST /api/auth/login**
```json
// Request
{ "email": "admin@cinema.com", "password": "admin" }

// Response 200
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "abc123...",
  "email": "admin@cinema.com",
  "name": "Administrador",
  "role": "Admin"
}

// Response 401
{ "type": "...", "title": "Não autorizado", "status": 401, "detail": "Email ou senha inválidos." }
```

**POST /api/auth/refresh**
```json
// Request
{ "refreshToken": "abc123..." }

// Response 200 — mesmo formato do login (token novo + refresh token novo)
// Response 401 — refresh token inválido, expirado ou já revogado
```

---

### 🔓 Sessões (público)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/sessions` | Lista sessões ativas com paginação |
| `GET` | `/api/sessions/{id}` | Detalhes da sessão com mapa de assentos |

**GET /api/sessions?date=2026-08-01&page=1&pageSize=20**
```json
// Response 200
{
  "items": [
    {
      "id": 1,
      "movieId": 1,
      "roomId": 1,
      "startTime": "2026-08-01T14:00:00Z",
      "endTime": "2026-08-01T17:00:00Z",
      "ticketPrice": 35.00,
      "movie": { "id": 1, "title": "Oppenheimer", "description": "...", "genre": "Drama/Biografia", "durationMinutes": 180, "posterUrl": "..." },
      "room": { "id": 1, "name": "Sala 1", "rows": 5, "columns": 4 }
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 4,
  "totalPages": 1,
  "hasPrevious": false,
  "hasNext": false
}
```

**GET /api/sessions/1**
```json
// Response 200
{
  "id": 1,
  "movieId": 1,
  "roomId": 1,
  "startTime": "2026-08-01T14:00:00Z",
  "endTime": "2026-08-01T17:00:00Z",
  "ticketPrice": 35.00,
  "movie": { ... },
  "room": { ... },
  "seats": [
    { "id": 1, "roomId": 1, "row": "A", "number": 1, "label": "A1", "isOccupied": false },
    { "id": 2, "roomId": 1, "row": "A", "number": 2, "label": "A2", "isOccupied": true }
  ]
}

// Response 404 — sessão não encontrada
```

---

### 🔒 Reservas (requer autenticação — User ou Admin)

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/reservations` | Cria reserva de assentos |

**POST /api/reservations** (Header: `Authorization: Bearer <token>`)
```json
// Request
{
  "sessionId": 1,
  "customerName": "João Silva",
  "seatIds": [1, 2, 3]
}

// Response 201
{
  "id": 1,
  "sessionId": 1,
  "customerName": "João Silva",
  "reservedAt": "2026-08-01T10:30:00Z",
  "tickets": [
    { "id": 1, "reservationId": 1, "sessionId": 1, "seatId": 1, "seat": { "id": 1, ... } },
    { "id": 2, "reservationId": 1, "sessionId": 1, "seatId": 2, "seat": { "id": 2, ... } }
  ]
}

// Response 409 — assento já ocupado (double-booking)
// Response 404 — sessão ou assento não encontrado
// Response 400 — validação (campos obrigatórios, mínimo 1 assento)
```

---

### 🔒 Admin — Filmes (requer perfil Admin)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/admin/movies` | Lista todos os filmes |
| `POST` | `/api/admin/movies` | Cadastra novo filme |
| `PUT` | `/api/admin/movies/{id}` | Atualiza dados de um filme |
| `DELETE` | `/api/admin/movies/{id}` | Exclui filme e suas sessões (hard delete) |

**POST /api/admin/movies**
```json
// Request
{
  "title": "Matrix",
  "description": "Um hacker descobre a verdade sobre sua realidade...",
  "genre": "Ficção Científica",
  "durationMinutes": 136,
  "posterUrl": "https://..."
}

// Response 201 — MovieDto
```

**DELETE /api/admin/movies/1**
```
// Response 204 No Content
// Response 404 — filme não encontrado
```

---

### 🔒 Admin — Salas (requer perfil Admin)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/admin/rooms` | Lista todas as salas |
| `POST` | `/api/admin/rooms` | Cria sala com geração automática de assentos |
| `DELETE` | `/api/admin/rooms/{id}` | Exclui sala, assentos e sessões (hard delete) |

**POST /api/admin/rooms**
```json
// Request
{ "name": "Sala VIP", "rows": 6, "columns": 5 }

// Response 201 — RoomDto (assentos gerados automaticamente: 6 fileiras × 5 colunas = 30 assentos)
```

---

### 🔒 Admin — Sessões (requer perfil Admin)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/admin/sessions` | Lista todas as sessões (inclui desativadas) com paginação |
| `GET` | `/api/admin/sessions/{id}/seats` | Mapa de assentos com nome do cliente e horário da reserva |
| `POST` | `/api/admin/sessions` | Cria nova sessão (valida conflito de horário na mesma sala) |
| `DELETE` | `/api/admin/sessions/{id}` | Desativa sessão (soft-delete) |
| `POST` | `/api/admin/sessions/{id}/restore` | Restaura sessão desativada |

**POST /api/admin/sessions**
```json
// Request
{
  "movieId": 1,
  "roomId": 1,
  "startTime": "2026-08-01T14:00:00Z",
  "endTime": "2026-08-01T17:00:00Z",
  "ticketPrice": 35.00
}

// Response 201 — SessionAdminDto
// Response 409 — conflito de horário na mesma sala
// Response 400 — validação (startTime deve ser anterior a endTime, etc.)
```

**GET /api/admin/sessions/1/seats**
```json
// Response 200
[
  { "id": 1, "roomId": 1, "row": "A", "number": 1, "label": "A1", "isOccupied": true, "customerName": "João Silva", "reservedAt": "2026-08-01T10:30:00Z" },
  { "id": 2, "roomId": 1, "row": "A", "number": 2, "label": "A2", "isOccupied": false, "customerName": null, "reservedAt": null }
]
```

**DELETE /api/admin/sessions/1**
```
// Response 204 No Content — sessão marcada como IsDeleted=true (soft-delete)
// Response 404 — sessão não encontrada
```

**POST /api/admin/sessions/1/restore**
```
// Response 204 No Content — sessão restaurada
// Response 409 — conflito de horário com sessão ativa na mesma sala
// Response 404 — sessão não encontrada
```

---

### ❤️ Health Check

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/health` | Health check da API |

---

## 🖥️ Rotas do Frontend (Angular)

| Rota | Componente | Acesso |
|------|-----------|--------|
| `/` | `SessionListComponent` | Público — sessões com navegação por 7 dias, filtros de gênero e sinopse |
| `/login` | `LoginComponent` | Público — login |
| `/booking/:id` | `BookingComponent` | Apenas User — reserva de assentos (bloqueado para Admin) |
| `/admin` | `AdminDashboardComponent` | Admin — dashboard com layout responsivo |
| `/admin/movies` | `MovieFormComponent` | Admin — CRUD filmes com edição inline |
| `/admin/rooms` | `RoomFormComponent` | Admin — CRUD salas |
| `/admin/sessions` | `SessionFormComponent` | Admin — CRUD sessões |
| `/admin/sessions/:id/seats` | `AdminSessionSeatsComponent` | Admin — mapa de assentos com dados de reserva |

---

## 🗄️ Modelo de Dados

```
┌──────────┐    ┌──────────┐    ┌───────────┐
│  Movie   │    │   Room   │    │   Seat    │
├──────────┤    ├──────────┤    ├───────────┤
│ Id       │    │ Id       │    │ Id        │
│ Title    │    │ Name     │    │ RoomId FK │
│ Descr…   │    │ Rows     │    │ Row       │
│ Genre    │    │ Columns  │    │ Number    │
│ Duration │    └────┬─────┘    │ Label*    │
│ PosterUrl│         │          └─────┬─────┘
└────┬─────┘         │                │
     │               │                │
     └──────┐   ┌────┘                │
            │   │                     │
       ┌────┴───┴─────┐               │
       │   Session    │               │
       ├──────────────┤               │
       │ Id           │               │
       │ MovieId FK   │               │
       │ RoomId FK    │               │
       │ StartTime    │               │
       │ EndTime      │               │
       │ TicketPrice  │               │
       │ IsDeleted    │               │
       └────┬────┬────┘               │
            │    │                    │
   ┌────────┘    └─────────┐          │
   │                       │          │
┌──┴──────────┐   ┌───────┴──────┐   │
│ Reservation │   │    Ticket    │   │
├─────────────┤   ├──────────────┤   │
│ Id          │   │ Id           │   │
│ SessionId FK│   │ ReservationId│   │
│ CustomerName│   │ SessionId FK │   │
│ ReservedAt  │   │ SeatId FK ───┼───┘
└─────────────┘   └──────────────┘

┌──────────────┐   ┌────────────────┐
│    User      │   │  RefreshToken  │
├──────────────┤   ├────────────────┤
│ Id           │   │ Id             │
│ Name         │   │ UserId FK      │
│ Email (UQ)   │   │ Token (UQ)     │
│ PasswordHash │   │ ExpiresAt      │
│ Role         │   │ CreatedAt      │
└──────────────┘   │ IsRevoked      │
                   └────────────────┘
```

**Índices e constraints:**
- `Ticket(SessionId, SeatId)` — **Unique Index** (impede double-booking no banco)
- `Seat(RoomId, Row, Number)` — **Unique Index** (assento único por sala)
- `User(Email)` — **Unique Index** (email único)
- `RefreshToken(Token)` — **Unique Index**
- Soft-delete em `Session` via coluna `IsDeleted` (sessões desativadas não aparecem para usuários)

---

## 🛡️ Tratamento de Erros

A API usa **ProblemDetails (RFC 7807)** para todos os erros, com middleware global (`GlobalExceptionHandler`):

| Código | Situação |
|--------|----------|
| `400` | Erro de validação (campos obrigatórios, formato inválido) |
| `401` | Credenciais inválidas ou token expirado |
| `404` | Recurso não encontrado (sessão, filme, sala, assento) |
| `409` | Conflito — assento já ocupado ou conflito de horário |

**Validação com FluentValidation:**
- `LoginValidator` — email e senha obrigatórios
- `CreateReservationValidator` — `SessionId > 0`, `CustomerName` obrigatório, `SeatIds` com mínimo 1
- `CreateMovieValidator` — título obrigatório (1-200), `DurationMinutes > 0`
- `CreateRoomValidator` — nome obrigatório (1-100), `Rows > 0`, `Columns > 0`
- `CreateSessionValidator` — `StartTime < EndTime`, `TicketPrice > 0`

---

## 📁 Estrutura do Projeto

```
desafio/
├── Cinema.Api/                          # Backend .NET 8
│   ├── Controllers/                     # Endpoints HTTP
│   │   ├── AuthController.cs            #   POST login, POST refresh
│   │   ├── SessionsController.cs        #   GET list, GET detail (público)
│   │   ├── ReservationsController.cs    #   POST create (auth)
│   │   ├── AdminMoviesController.cs     #   GET, POST, DELETE /api/admin/movies
│   │   ├── AdminRoomsController.cs      #   GET, POST, DELETE /api/admin/rooms
│   │   └── AdminSessionsController.cs   #   GET, POST, DELETE, restore, seats
│   ├── Models/                          # Entidades de domínio
│   │   ├── Movie.cs                     #   Filme
│   │   ├── Room.cs                      #   Sala
│   │   ├── Seat.cs                      #   Assento (Row + Number, Label)
│   │   ├── Session.cs                   #   Sessão (com IsDeleted soft-delete)
│   │   ├── Reservation.cs               #   Reserva
│   │   ├── Ticket.cs                    #   Ingresso (SessionId + SeatId unique)
│   │   ├── User.cs                      #   Usuário (Email único, Role)
│   │   ├── RefreshToken.cs              #   Refresh token (rotação)
│   │   └── PagedResult.cs               #   Resultado paginado genérico
│   ├── DTOs/                            # Objetos de transferência
│   │   ├── LoginRequest.cs / LoginResponse.cs / RefreshTokenRequest.cs
│   │   ├── SessionDto.cs / SessionDetailDto.cs / SeatDto.cs / AdminSeatDto.cs
│   │   ├── CreateReservationRequest.cs / ReservationResponse.cs
│   │   ├── CreateMovieRequest.cs / CreateRoomRequest.cs / CreateSessionRequest.cs
│   │   └── MovieDto.cs / RoomDto.cs
│   ├── Services/                        # Lógica de negócio (SOLID)
│   │   ├── IAuthService.cs / AuthService.cs
│   │   ├── ITokenService.cs / TokenService.cs
│   │   ├── ISessionService.cs / SessionService.cs
│   │   ├── IReservationService.cs / ReservationService.cs
│   │   ├── IMovieService.cs / MovieService.cs
│   │   └── IRoomService.cs / RoomService.cs
│   ├── Repositories/                    # Acesso a dados (Repository Pattern)
│   │   ├── ISessionRepository.cs / SessionRepository.cs
│   │   ├── IReservationRepository.cs / ReservationRepository.cs
│   │   ├── IMovieRepository.cs / MovieRepository.cs
│   │   ├── IRoomRepository.cs / RoomRepository.cs
│   │   └── IUserRepository.cs / UserRepository.cs
│   ├── Validators/                      # FluentValidation
│   │   ├── LoginValidator.cs
│   │   ├── CreateReservationValidator.cs
│   │   ├── CreateMovieValidator.cs
│   │   ├── CreateRoomValidator.cs
│   │   └── CreateSessionValidator.cs
│   ├── Data/                            # EF Core + Migrations
│   │   ├── AppDbContext.cs              #   DbContext + OnModelCreating + Seed
│   │   ├── IUnitOfWork.cs / UnitOfWork.cs
│   │   ├── DataSeeder.cs                #   Seed de usuários (BCrypt runtime)
│   │   └── Migrations/                  #   5 migrations
│   ├── Exceptions/                      # DomainException + subclasses
│   │   └── DomainException.cs           #   SeatAlreadyOccupied, SessionNotFound, etc.
│   ├── Middleware/                       # Pipeline HTTP
│   │   └── GlobalExceptionHandler.cs    #   ProblemDetails + unique-constraint detection
│   ├── Program.cs                       #   Config: DI, JWT, Swagger, CORS, EF, Validation
│   ├── Cinema.Api.csproj                #   Projeto + pacotes NuGet
│   └── Properties/launchSettings.json   #   Profile http (5125) + https (7209)
├── cinema-app/                          # Frontend Angular 18
│   ├── src/
│   │   ├── app/
│   │   │   ├── pages/
│   │   │   │   ├── sessions/            #   SessionListComponent (público)
│   │   │   │   ├── login/               #   LoginComponent
│   │   │   │   ├── booking/             #   BookingComponent (reserva)
│   │   │   │   └── admin/
│   │   │   │       ├── admin.component.ts
│   │   │   │       ├── dashboard/       #   AdminDashboardComponent
│   │   │   │       ├── movie-form/      #   MovieFormComponent
│   │   │   │       ├── room-form/       #   RoomFormComponent
│   │   │   │       ├── session-form/    #   SessionFormComponent
│   │   │   │       └── session-seats/   #   AdminSessionSeatsComponent
│   │   │   ├── services/
│   │   │   │   ├── cinema.service.ts    #   API pública (sessions, reservations)
│   │   │   │   ├── auth.service.ts      #   Login, logout, refresh, localStorage
│   │   │   │   ├── admin.service.ts     #   API admin (movies, rooms, sessions)
│   │   │   │   └── toast.service.ts     #   Notificações toast
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts        #   Protege rotas que exigem login
│   │   │   │   └── admin.guard.ts       #   Protege rotas que exigem Admin
│   │   │   ├── interceptors/
│   │   │   │   ├── auth.interceptor.ts  #   Injeta Bearer token + refresh automático
│   │   │   │   └── error.interceptor.ts #   Tratamento global de erros HTTP
│   │   │   ├── models/
│   │   │   │   └── cinema.models.ts     #   Interfaces TypeScript
│   │   │   ├── components/
│   │   │   │   └── toast-container.component.ts
│   │   │   ├── app.routes.ts            #   Rotas (lazy loading, guards)
│   │   │   ├── app.config.ts            #   Config (providers, interceptors)
│   │   │   └── app.component.ts
│   │   ├── environments/
│   │   │   ├── environment.ts           #   apiUrl produção
│   │   │   └── environment.development.ts # apiUrl = http://localhost:5125/api
│   │   ├── index.html
│   │   ├── main.ts
│   │   └── styles.css
│   ├── angular.json                     #   Config Angular CLI
│   ├── package.json                     #   Dependências
│   └── tsconfig.json
└── README.md                            #   Este arquivo
```

---

## 🔄 Fluxo de Autenticação

```
1. POST /api/auth/login  →  { token, refreshToken, email, name, role }
2. Frontend armazena token + refreshToken em localStorage
3. Toda requisição autenticada → Header: Authorization: Bearer <token>
4. Se token expirar (401) → interceptor chama POST /api/auth/refresh
5. Refresh rotaciona o token → novo par (token, refreshToken)
6. Se refresh também falhar → redireciona para /login
```

---

## 🎨 Funcionalidades da Interface

### Página de Sessões
- **Navegação por dias**: Tira horizontal rolável com 7 dias a partir de hoje — cada dia exibe nome do dia da semana + número, com o dia atual destacado
- **Filtros de gênero**: Chips dinâmicos extraídos das sessões carregadas (Ação, Drama, Ficção, etc.) — filtro instantâneo sem recarregar
- **Sinopse**: Botão 📖 em cada card que abre modal com a descrição completa do filme
- **Horários**: Cards mostram apenas horário (HH:mm), já que a data está na tira de navegação
- **Reserva**: Botão "Reservar Assentos" visível apenas para usuários com perfil User

### Painel Admin
- **Layout responsivo**: Em desktop, sidebar lateral de 250px; em mobile (≤768px), topbar compacta com menu hamburger que desliza como overlay
- **Edição de filmes**: Botão ✏️ na tabela que pré-preenche o formulário para edição, com botão Cancelar para sair do modo de edição
- **Tabelas simplificadas**: Colunas de ID removidas das tabelas de filmes, salas e sessões
