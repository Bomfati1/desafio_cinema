using Cinema.Api.Models;
using Cinema.Api.Repositories;
using Cinema.Api.Services;

namespace Cinema.Api.Data;

/// <summary>
/// Inicializa dados que não podem ser seedados via HasData
/// (ex: usuários com hash BCrypt dinâmico).
/// </summary>
public static class DataSeeder
{
    public static async Task SeedUsersAsync(IUserRepository userRepo)
    {
        if (await userRepo.AnyAsync())
            return;

        var users = new List<User>
        {
            new()
            {
                Name = "Administrador",
                Email = "admin@cinema.com",
                PasswordHash = AuthService.HashPassword("admin"),
                Role = "Admin"
            },
            new()
            {
                Name = "Usuário Teste",
                Email = "user@email.com",
                PasswordHash = AuthService.HashPassword("user"),
                Role = "User"
            }
        };

        await userRepo.AddRangeAsync(users);
    }
}
