using Cinema.Api.Data;
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Repositories;

public interface IRoomRepository
{
    Task<List<Room>> GetAllAsync(CancellationToken ct = default);
    Task<Room?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Room> AddAsync(Room room, CancellationToken ct = default);
    Task AddSeatsAsync(List<Seat> seats, CancellationToken ct = default);
    Task<List<Seat>> GetSeatsByRoomAsync(int roomId, CancellationToken ct = default);
    Task DeleteAsync(Room room, CancellationToken ct = default);
}

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _db;

    public RoomRepository(AppDbContext db) => _db = db;

    public async Task<List<Room>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Rooms.ToListAsync(ct);
    }

    public async Task<Room?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Rooms.FindAsync([id], ct);
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

    public async Task DeleteAsync(Room room, CancellationToken ct = default)
    {
        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync(ct);
    }
}
