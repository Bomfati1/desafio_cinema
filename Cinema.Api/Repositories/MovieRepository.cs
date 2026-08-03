using Cinema.Api.Data;
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Repositories;

public interface IMovieRepository
{
    Task<List<Movie>> GetAllAsync(CancellationToken ct = default);
    Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Movie> AddAsync(Movie movie, CancellationToken ct = default);
    Task<Movie> UpdateAsync(Movie movie, CancellationToken ct = default);
    Task DeleteAsync(Movie movie, CancellationToken ct = default);
}

public class MovieRepository : IMovieRepository
{
    private readonly AppDbContext _db;

    public MovieRepository(AppDbContext db) => _db = db;

    public async Task<List<Movie>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Movies.ToListAsync(ct);
    }

    public async Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Movies.FindAsync([id], ct);
    }

    public async Task<Movie> AddAsync(Movie movie, CancellationToken ct = default)
    {
        _db.Movies.Add(movie);
        await _db.SaveChangesAsync(ct);
        return movie;
    }

    public async Task<Movie> UpdateAsync(Movie movie, CancellationToken ct = default)
    {
        _db.Movies.Update(movie);
        await _db.SaveChangesAsync(ct);
        return movie;
    }

    public async Task DeleteAsync(Movie movie, CancellationToken ct = default)
    {
        _db.Movies.Remove(movie);
        await _db.SaveChangesAsync(ct);
    }
}
