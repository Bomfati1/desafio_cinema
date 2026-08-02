using Cinema.Api.Data;
using Cinema.Api.Models;
using Cinema.Api.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cinema.Api.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"IntegrationTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Fornece valores dummy para os secrets exigidos pelo Program.cs
        builder.UseSetting("Jwt:Key", "IntegrationTestKeyWithMinimum32Chars!!");
        builder.UseSetting("ConnectionStrings:DefaultConnection",
            "Host=test;Port=5432;Database=test;Username=test;Password=test");

        builder.ConfigureServices(services =>
        {
            // Substitui PostgreSQL por InMemory nos testes de integração
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            var dbName = _dbName; // mesma instância para factory + app
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });

            // Aplica o schema e popula dados de teste
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            // Seed: movie + room + session para testes de listagem
            if (!db.Movies.Any())
            {
                var movie = new Movie
                {
                    Id = 1,
                    Title = "Filme Teste",
                    Description = "Descrição do filme de teste",
                    DurationMinutes = 120,
                    Genre = "Ação"
                };
                db.Movies.Add(movie);

                var room = new Room
                {
                    Id = 1,
                    Name = "Sala 1",
                    Rows = 5,
                    Columns = 6
                };
                db.Rooms.Add(room);

                db.Sessions.Add(new Session
                {
                    Id = 1,
                    MovieId = 1,
                    RoomId = 1,
                    StartTime = DateTime.UtcNow.AddDays(1),
                    EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                    TicketPrice = 25.00m
                });

                db.SaveChanges();
            }
        });
    }
}
