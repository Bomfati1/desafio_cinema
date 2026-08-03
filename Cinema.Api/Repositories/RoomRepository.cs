using Cinema.Api.Data;
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Repositories;

public interface IRoomRepository
{
    Task<List<Room>> GetAllAsync(bool includeDeleted = false, CancellationToken ct = default);
    Task<Room?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default);
    Task<Room> AddAsync(Room room, CancellationToken ct = default);
    Task AddSeatsAsync(List<Seat> seats, CancellationToken ct = default);
    Task<List<Seat>> GetSeatsByRoomAsync(int roomId, CancellationToken ct = default);
    Task UpdateAsync(Room room, CancellationToken ct = default);
}

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _db;

    public RoomRepository(AppDbContext db) => _db = db;

    public async Task<List<Room>> GetAllAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _db.Rooms.AsQueryable();
        if (!includeDeleted)
            query = query.Where(r => !r.IsDeleted);
        return await query.OrderBy(r => r.Name).ToListAsync(ct);
    }

    public async Task<Room?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _db.Rooms.AsQueryable();
        if (!includeDeleted)
            query = query.Where(r => !r.IsDeleted);
        return await query.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Room> AddAsync(Room room, CancellationToken ct = default)
    {
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(ct);
        return room;
    }

    public async Task AddSeatsAsync(List<Seat> seats, CancellationToken ct = default)
    {
        _db.Seats.AddRange(seats);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Seat>> GetSeatsByRoomAsync(int roomId, CancellationToken ct = default)
    {
        return await _db.Seats
            .Where(s => s.RoomId == roomId)
            .OrderBy(s => s.Row)
            .ThenBy(s => s.Number)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(Room room, CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
