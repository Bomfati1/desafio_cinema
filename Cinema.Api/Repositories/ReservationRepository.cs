using Cinema.Api.Data;
using Cinema.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Repositories;

public interface IReservationRepository
{
    Task<List<int>> GetOccupiedSeatIdsAsync(int sessionId, CancellationToken ct = default);
    Task<List<int>> GetAlreadyOccupiedAsync(int sessionId, List<int> seatIds, CancellationToken ct = default);
    Task<Reservation> AddReservationAsync(Reservation reservation, CancellationToken ct = default);
    Task AddTicketsAsync(List<Ticket> tickets, CancellationToken ct = default);
    Task<Session?> GetSessionWithRoomAsync(int sessionId, CancellationToken ct = default);
    Task<List<Seat>> GetSeatsByIdsAsync(List<int> seatIds, int roomId, CancellationToken ct = default);
    Task<List<Ticket>> GetTicketsWithReservationsAsync(int sessionId, CancellationToken ct = default);
}

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _db;

    public ReservationRepository(AppDbContext db) => _db = db;

    public async Task<List<int>> GetOccupiedSeatIdsAsync(int sessionId, CancellationToken ct = default)
    {
        return await _db.Tickets
            .Where(t => t.SessionId == sessionId)
            .Select(t => t.SeatId)
            .ToListAsync(ct);
    }

    public async Task<List<int>> GetAlreadyOccupiedAsync(int sessionId, List<int> seatIds, CancellationToken ct = default)
    {
        return await _db.Tickets
            .Where(t => t.SessionId == sessionId && seatIds.Contains(t.SeatId))
            .Select(t => t.SeatId)
            .ToListAsync(ct);
    }

    public async Task<Reservation> AddReservationAsync(Reservation reservation, CancellationToken ct = default)
    {
        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync(ct);
        return reservation;
    }

    public async Task AddTicketsAsync(List<Ticket> tickets, CancellationToken ct = default)
    {
        _db.Tickets.AddRange(tickets);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Session?> GetSessionWithRoomAsync(int sessionId, CancellationToken ct = default)
    {
        return await _db.Sessions
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted, ct);
    }

    public async Task<List<Seat>> GetSeatsByIdsAsync(List<int> seatIds, int roomId, CancellationToken ct = default)
    {
        return await _db.Seats
            .Where(s => seatIds.Contains(s.Id) && s.RoomId == roomId)
            .ToListAsync(ct);
    }

    public async Task<List<Ticket>> GetTicketsWithReservationsAsync(int sessionId, CancellationToken ct = default)
    {
        return await _db.Tickets
            .Include(t => t.Reservation)
            .Where(t => t.SessionId == sessionId)
            .ToListAsync(ct);
    }
}
