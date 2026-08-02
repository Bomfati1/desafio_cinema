using Cinema.Api.Data;
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
    Task AddRangeAsync(List<User> users, CancellationToken ct = default);
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<bool> AnyAsync(CancellationToken ct = default)
    {
        return await _db.Users.AnyAsync(ct);
    }

    public async Task AddRangeAsync(List<User> users, CancellationToken ct = default)
    {
        _db.Users.AddRange(users);
        await _db.SaveChangesAsync(ct);
    }
}
