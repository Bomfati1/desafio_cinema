using Cinema.Api.Data;
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Repositories;

public interface ISessionRepository
{
    Task<List<Session>> GetSessionsAsync(DateTime? date, bool includeDeleted, CancellationToken ct = default);
    Task<(List<Session> Items, int TotalCount)> GetSessionsPagedAsync(DateTime? date, bool includeDeleted, int page, int pageSize, CancellationToken ct = default);
    Task<Session?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default);
    Task<Session?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<Session> AddAsync(Session session, CancellationToken ct = default);
    Task<bool> HasOverlapAsync(int roomId, DateTime start, DateTime end, int? excludeId = null, CancellationToken ct = default);
    Task UpdateAsync(Session session, CancellationToken ct = default);
}

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _db;

    public SessionRepository(AppDbContext db) => _db = db;

    public async Task<List<Session>> GetSessionsAsync(DateTime? date, bool includeDeleted, CancellationToken ct = default)
    {
        var query = _db.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .AsQueryable();

        if (!includeDeleted)
            query = query.Where(s => !s.IsDeleted);

        if (date.HasValue)
        {
            var startOfDay = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);
            query = query.Where(s => s.StartTime >= startOfDay && s.StartTime < endOfDay);
        }

        return await query
            .OrderBy(s => s.Room != null ? s.Room.Name : "")
            .ThenBy(s => s.StartTime)
            .ToListAsync(ct);
    }

    public async Task<Session?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _db.Sessions.AsQueryable();
        if (!includeDeleted)
            query = query.Where(s => !s.IsDeleted);
        return await query.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Session?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _db.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);
    }

    public async Task<Session> AddAsync(Session session, CancellationToken ct = default)
    {
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<bool> HasOverlapAsync(int roomId, DateTime start, DateTime end, int? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Sessions
            .Where(s => s.RoomId == roomId && !s.IsDeleted && s.StartTime < end && s.EndTime > start);

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<(List<Session> Items, int TotalCount)> GetSessionsPagedAsync(DateTime? date, bool includeDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .AsQueryable();

        if (!includeDeleted)
            query = query.Where(s => !s.IsDeleted);

        if (date.HasValue)
        {
            var startOfDay = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);
            query = query.Where(s => s.StartTime >= startOfDay && s.StartTime < endOfDay);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(s => s.Room != null ? s.Room.Name : "")
            .ThenBy(s => s.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task UpdateAsync(Session session, CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
