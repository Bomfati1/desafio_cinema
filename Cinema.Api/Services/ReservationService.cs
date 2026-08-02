using Cinema.Api.Data;
using Cinema.Api.DTOs;
using Cinema.Api.Exceptions;
using Cinema.Api.Models;
using Cinema.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(IReservationRepository repo, IUnitOfWork uow, ILogger<ReservationService> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request)
    {
        var session = await _repo.GetSessionWithRoomAsync(request.SessionId)
            ?? throw new SessionNotFoundException(request.SessionId);

        var seats = await _repo.GetSeatsByIdsAsync(request.SeatIds, session.Room!.Id);

        if (seats.Count != request.SeatIds.Count)
        {
            var missing = request.SeatIds.Except(seats.Select(s => s.Id));
            throw new SeatNotFoundException(missing.First());
        }

        using var tx = await _uow.BeginTransactionAsync();

        try
        {
            var alreadyOccupied = await _repo.GetAlreadyOccupiedAsync(request.SessionId, request.SeatIds);

            if (alreadyOccupied.Count > 0)
                throw new SeatAlreadyOccupiedException(request.SessionId, alreadyOccupied.First());

            var reservation = new Reservation
            {
                SessionId = request.SessionId,
                CustomerName = request.CustomerName.Trim(),
                ReservedAt = DateTime.UtcNow
            };

            await _repo.AddReservationAsync(reservation);

            var tickets = seats.Select(seat => new Ticket
            {
                ReservationId = reservation.Id,
                SessionId = request.SessionId,
                SeatId = seat.Id
            }).ToList();

            await _repo.AddTicketsAsync(tickets);
            await tx.CommitAsync();

            _logger.LogInformation(
                "Reserva #{ReservationId} criada: {Customer}, Sessão {SessionId}, {Count} assentos",
                reservation.Id, request.CustomerName, request.SessionId, tickets.Count);

            // Build seat lookup once for O(n) projection
            var seatLookup = seats.ToDictionary(s => s.Id);

            var ticketDtos = tickets.Select(t =>
            {
                var s = seatLookup[t.SeatId];
                return new TicketDto(
                    t.Id, t.ReservationId, t.SessionId, t.SeatId,
                    new SeatDto(s.Id, s.RoomId, s.Row, s.Number, s.Label, true));
            }).ToList();

            return new ReservationResponse(
                reservation.Id, reservation.SessionId, reservation.CustomerName,
                reservation.ReservedAt, ticketDtos);
        }
        catch (DomainException)
        {
            await tx.RollbackAsync();
            throw;
        }
        catch (Exception)
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
