using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ──────────────────────────────────────────────
        // Movie
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(2000);
            entity.Property(m => m.Genre).HasMaxLength(100);
            entity.Property(m => m.PosterUrl).HasMaxLength(500);
        });

        // ──────────────────────────────────────────────
        // Room
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
        });

        // ──────────────────────────────────────────────
        // Seat
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Row).IsRequired().HasMaxLength(2);

            // Um assento é único dentro de uma sala (Row + Number)
            entity.HasIndex(s => new { s.RoomId, s.Row, s.Number }).IsUnique();

            entity.HasOne(s => s.Room)
                  .WithMany(r => r.Seats)
                  .HasForeignKey(s => s.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ──────────────────────────────────────────────
        // Session
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.TicketPrice).HasColumnType("decimal(10,2)");

            entity.HasOne(s => s.Movie)
                  .WithMany(m => m.Sessions)
                  .HasForeignKey(s => s.MovieId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Room)
                  .WithMany(r => r.Sessions)
                  .HasForeignKey(s => s.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ──────────────────────────────────────────────
        // Reservation
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(r => r.ReservedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(r => r.Session)
                  .WithMany(s => s.Reservations)
                  .HasForeignKey(r => r.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ──────────────────────────────────────────────
        // Ticket  (CRUCIAL: Unique Index contra dupla reserva)
        // ──────────────────────────────────────────────
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(t => t.Id);

            // ⚠️ Garantia de consistência: o mesmo assento não pode ser
            //    reservado duas vezes na mesma sessão.
            entity.HasIndex(t => new { t.SessionId, t.SeatId }).IsUnique();

            entity.HasOne(t => t.Reservation)
                  .WithMany(r => r.Tickets)
                  .HasForeignKey(t => t.ReservationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Session)
                  .WithMany(s => s.Tickets)
                  .HasForeignKey(t => t.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Seat)
                  .WithMany(s => s.Tickets)
                  .HasForeignKey(t => t.SeatId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ──────────────────────────────────────────────
        // User (JWT Authentication)
        // ──────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
        });

        // ──────────────────────────────────────────────
        // RefreshToken
        // ──────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(256);
            entity.HasIndex(rt => rt.Token).IsUnique();

            entity.HasOne(rt => rt.User)
                  .WithMany()
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ──────────────────────────────────────────────
        // Data Seeding – MVP
        // ──────────────────────────────────────────────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // ── Room ──────────────────────────────────────
        var room = new Room
        {
            Id = 1,
            Name = "Sala 1",
            Rows = 5,
            Columns = 4
        };
        modelBuilder.Entity<Room>().HasData(room);

        // ── Seats (20 assentos: 5 fileiras × 4 colunas) ──
        var seats = new List<Seat>();
        int seatId = 1;
        string[] rows = { "A", "B", "C", "D", "E" };
        for (int r = 0; r < 5; r++)
        {
            for (int c = 1; c <= 4; c++)
            {
                seats.Add(new Seat
                {
                    Id = seatId++,
                    RoomId = 1,
                    Row = rows[r],
                    Number = c
                });
            }
        }
        modelBuilder.Entity<Seat>().HasData(seats);

        // ── Movies ────────────────────────────────────
        var movies = new[]
        {
            new Movie
            {
                Id = 1,
                Title = "Oppenheimer",
                Description = "A história do físico J. Robert Oppenheimer e seu papel no desenvolvimento da bomba atômica.",
                Genre = "Drama/Biografia",
                DurationMinutes = 180,
                PosterUrl = "https://image.tmdb.org/t/p/w500/8Gxv8gSFCU0XGDykEGv7z0I4E2K.jpg"
            },
            new Movie
            {
                Id = 2,
                Title = "Duna: Parte 2",
                Description = "Paul Atreides se une aos Fremen em uma jornada de vingança contra os conspiradores que destruíram sua família.",
                Genre = "Ficção Científica",
                DurationMinutes = 166,
                PosterUrl = "https://image.tmdb.org/t/p/w500/8b8R8l1QmXLXQm3Fk0M3nL8M7yT.jpg"
            }
        };
        modelBuilder.Entity<Movie>().HasData(movies);

        // ── Sessions (4 sessões: 2 por filme) ─────────
        var baseDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        var sessions = new[]
        {
            new Session
            {
                Id = 1,
                MovieId = 1,
                RoomId = 1,
                StartTime = baseDate.AddHours(14),       // 14:00
                EndTime = baseDate.AddHours(17),          // 17:00 (180 min)
                TicketPrice = 35.00m,
                IsDeleted = false
            },
            new Session
            {
                Id = 2,
                MovieId = 1,
                RoomId = 1,
                StartTime = baseDate.AddHours(19),       // 19:00
                EndTime = baseDate.AddHours(22),          // 22:00
                TicketPrice = 40.00m,
                IsDeleted = false
            },
            new Session
            {
                Id = 3,
                MovieId = 2,
                RoomId = 1,
                StartTime = baseDate.AddHours(15),       // 15:00
                EndTime = baseDate.AddHours(17).AddMinutes(46), // 17:46 (166 min)
                TicketPrice = 35.00m,
                IsDeleted = false
            },
            new Session
            {
                Id = 4,
                MovieId = 2,
                RoomId = 1,
                StartTime = baseDate.AddHours(20),       // 20:00
                EndTime = baseDate.AddHours(22).AddMinutes(46), // 22:46
                TicketPrice = 40.00m,
                IsDeleted = false
            }
        };
        modelBuilder.Entity<Session>().HasData(sessions);

        // ── Users: seed movido para DataSeeder (runtime) ──
        // Hashes BCrypt são dinâmicos (salt aleatório) e não
        // podem ser constantes no HasData do EF Core.
    }
}
