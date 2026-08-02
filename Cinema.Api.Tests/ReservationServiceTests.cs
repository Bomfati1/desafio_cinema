using Cinema.Api.Data;
using Cinema.Api.DTOs;
using Cinema.Api.Exceptions;
using Cinema.Api.Models;
using Cinema.Api.Repositories;
using Cinema.Api.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cinema.Api.Tests;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ILogger<ReservationService>> _logger = new();
    private readonly Mock<IDbContextTransaction> _tx = new();

    public ReservationServiceTests()
    {
        _uow.Setup(u => u.BeginTransactionAsync(default)).ReturnsAsync(_tx.Object);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
    }

    // ── Session not found ──────────────────────────────────

    [Fact]
    public async Task CreateReservation_ThrowsWhenSessionNotFound()
    {
        _repo.Setup(r => r.GetSessionWithRoomAsync(99, default)).ReturnsAsync((Session?)null);
        var sut = new ReservationService(_repo.Object, _uow.Object, _logger.Object);

        await Assert.ThrowsAsync<SessionNotFoundException>(() =>
            sut.CreateReservationAsync(new CreateReservationRequest(99, "João", new List<int> { 1 })));
    }

    // ── Seat not found ─────────────────────────────────────

    [Fact]
    public async Task CreateReservation_ThrowsWhenSeatMissing()
    {
        var session = new Session { Id = 1, Room = new Room { Id = 1 } };
        _repo.Setup(r => r.GetSessionWithRoomAsync(1, default)).ReturnsAsync(session);
        _repo.Setup(r => r.GetSeatsByIdsAsync(new List<int> { 99 }, 1, default))
             .ReturnsAsync(new List<Seat>());

        var sut = new ReservationService(_repo.Object, _uow.Object, _logger.Object);

        await Assert.ThrowsAsync<SeatNotFoundException>(() =>
            sut.CreateReservationAsync(new CreateReservationRequest(1, "João", new List<int> { 99 })));
    }

    // ── Seat already occupied ──────────────────────────────

    [Fact]
    public async Task CreateReservation_ThrowsWhenSeatAlreadyOccupied()
    {
        var session = new Session { Id = 1, Room = new Room { Id = 1 } };
        var seat = new Seat { Id = 5, RoomId = 1, Row = "A", Number = 5 };

        _repo.Setup(r => r.GetSessionWithRoomAsync(1, default)).ReturnsAsync(session);
        _repo.Setup(r => r.GetSeatsByIdsAsync(new List<int> { 5 }, 1, default))
             .ReturnsAsync(new List<Seat> { seat });
        _repo.Setup(r => r.GetAlreadyOccupiedAsync(1, new List<int> { 5 }, default))
             .ReturnsAsync(new List<int> { 5 });

        var sut = new ReservationService(_repo.Object, _uow.Object, _logger.Object);

        await Assert.ThrowsAsync<SeatAlreadyOccupiedException>(() =>
            sut.CreateReservationAsync(new CreateReservationRequest(1, "Maria", new List<int> { 5 })));
    }

    // ── Happy path ─────────────────────────────────────────

    [Fact]
    public async Task CreateReservation_Success_ReturnsReservationResponse()
    {
        var session = new Session { Id = 1, Room = new Room { Id = 1 } };
        var seat = new Seat { Id = 3, RoomId = 1, Row = "A", Number = 3 };

        _repo.Setup(r => r.GetSessionWithRoomAsync(1, default)).ReturnsAsync(session);
        _repo.Setup(r => r.GetSeatsByIdsAsync(new List<int> { 3 }, 1, default))
             .ReturnsAsync(new List<Seat> { seat });
        _repo.Setup(r => r.GetAlreadyOccupiedAsync(1, new List<int> { 3 }, default))
             .ReturnsAsync(new List<int>());
        _repo.Setup(r => r.AddReservationAsync(It.IsAny<Reservation>(), default))
             .Callback<Reservation, CancellationToken>((r, _) => r.Id = 100);
        _repo.Setup(r => r.AddTicketsAsync(It.IsAny<List<Ticket>>(), default))
             .Callback<List<Ticket>, CancellationToken>((tickets, _) =>
             {
                 for (int i = 0; i < tickets.Count; i++)
                     tickets[i].Id = 200 + i;
             });

        var sut = new ReservationService(_repo.Object, _uow.Object, _logger.Object);

        var result = await sut.CreateReservationAsync(
            new CreateReservationRequest(1, "  João Silva  ", new List<int> { 3 }));

        Assert.Equal(100, result.Id);
        Assert.Equal(1, result.SessionId);
        Assert.Equal("João Silva", result.CustomerName);
        Assert.Single(result.Tickets);
        Assert.Equal(3, result.Tickets[0].SeatId);
        Assert.NotNull(result.Tickets[0].Seat);
        Assert.True(result.Tickets[0].Seat!.IsOccupied);
    }

    // ── Multiple seats ─────────────────────────────────────

    [Fact]
    public async Task CreateReservation_MultipleSeats_ReturnsAllTickets()
    {
        var session = new Session { Id = 2, Room = new Room { Id = 2 } };
        var seats = new List<Seat>
        {
            new() { Id = 1, RoomId = 2, Row = "A", Number = 1 },
            new() { Id = 2, RoomId = 2, Row = "A", Number = 2 },
            new() { Id = 3, RoomId = 2, Row = "A", Number = 3 }
        };

        _repo.Setup(r => r.GetSessionWithRoomAsync(2, default)).ReturnsAsync(session);
        _repo.Setup(r => r.GetSeatsByIdsAsync(new List<int> { 1, 2, 3 }, 2, default))
             .ReturnsAsync(seats);
        _repo.Setup(r => r.GetAlreadyOccupiedAsync(2, new List<int> { 1, 2, 3 }, default))
             .ReturnsAsync(new List<int>());
        _repo.Setup(r => r.AddReservationAsync(It.IsAny<Reservation>(), default))
             .Callback<Reservation, CancellationToken>((r, _) => r.Id = 200);
        _repo.Setup(r => r.AddTicketsAsync(It.IsAny<List<Ticket>>(), default))
             .Callback<List<Ticket>, CancellationToken>((tickets, _) =>
             {
                 for (int i = 0; i < tickets.Count; i++)
                     tickets[i].Id = 300 + i;
             });

        var sut = new ReservationService(_repo.Object, _uow.Object, _logger.Object);

        var result = await sut.CreateReservationAsync(
            new CreateReservationRequest(2, "Ana", new List<int> { 1, 2, 3 }));

        Assert.Equal(200, result.Id);
        Assert.Equal(3, result.Tickets.Count);
        Assert.Contains(result.Tickets, t => t.SeatId == 1);
        Assert.Contains(result.Tickets, t => t.SeatId == 2);
        Assert.Contains(result.Tickets, t => t.SeatId == 3);
    }
}
