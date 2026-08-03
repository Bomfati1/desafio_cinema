using Cinema.Api.Data;
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Repositories;

public interface IMovieRepository
{
    Task<List<Movie>> GetAllAsync(bool includeDeleted = false, CancellationToken ct = default);
    Task<Movie?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default);
    Task<Movie> AddAsync(Movie movie, CancellationToken ct = default);
    Task UpdateAsync(Movie movie, CancellationToken ct = default);
}

public class MovieRepository : IMovieRepository
{
    private readonly AppDbContext _db;

    public MovieRepository(AppDbContext db) => _db = db;

    public async Task<List<Movie>> GetAllAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _db.Movies.AsQueryable();
        if (!includeDeleted)
            query = query.Where(m => !m.IsDeleted);
        return await query.OrderBy(m => m.Title).ToListAsync(ct);
    }

    public async Task<Movie?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _db.Movies.AsQueryable();
        if (!includeDeleted)
            query = query.Where(m => !m.IsDeleted);
        return await query.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<Movie> AddAsync(Movie movie, CancellationToken ct = default)
    {
        _db.Movies.Add(movie);
        await _db.SaveChangesAsync(ct);
        return movie;
    }

    public async Task UpdateAsync(Movie movie, CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
