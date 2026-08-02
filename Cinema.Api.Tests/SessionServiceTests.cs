using Cinema.Api.DTOs;
using Cinema.Api.Exceptions;
using Cinema.Api.Models;
using Cinema.Api.Repositories;
using Cinema.Api.Services;
using Moq;
using Xunit;

namespace Cinema.Api.Tests;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _sessionRepo = new();
    private readonly Mock<IMovieRepository> _movieRepo = new();
    private readonly Mock<IRoomRepository> _roomRepo = new();
    private readonly Mock<IReservationRepository> _reservationRepo = new();
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _sut = new SessionService(
            _sessionRepo.Object, _movieRepo.Object,
            _roomRepo.Object, _reservationRepo.Object);
    }

    // ── GetSessionsAsync ───────────────────────────────────

    [Fact]
    public async Task GetSessionsAsync_ReturnsOnlyNonDeleted()
    {
        var sessions = new List<Session>
        {
            new() { Id = 1, Movie = new Movie(), Room = new Room(), IsDeleted = false },
            new() { Id = 2, Movie = new Movie(), Room = new Room(), IsDeleted = true },
        };
        _sessionRepo
            .Setup(r => r.GetSessionsAsync(null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions.Where(s => !s.IsDeleted).ToList());

        var result = await _sut.GetSessionsAsync();
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    // ── CreateSessionAsync ─────────────────────────────────

    [Fact]
    public async Task CreateSessionAsync_ThrowsWhenMovieNotFound()
    {
        _movieRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((Movie?)null);

        var request = new CreateSessionRequest(99, 1,
            new DateTime(2026, 8, 1, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 1, 17, 0, 0, DateTimeKind.Utc),
            35.00m);

        await Assert.ThrowsAsync<MovieNotFoundException>(() => _sut.CreateSessionAsync(request));
    }

    [Fact]
    public async Task CreateSessionAsync_ThrowsWhenRoomNotFound()
    {
        _movieRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Movie { Id = 1 });
        _roomRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Room?)null);

        var request = new CreateSessionRequest(1, 99,
            new DateTime(2026, 8, 1, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 1, 17, 0, 0, DateTimeKind.Utc),
            35.00m);

        await Assert.ThrowsAsync<RoomNotFoundException>(() => _sut.CreateSessionAsync(request));
    }

    [Fact]
    public async Task CreateSessionAsync_ThrowsOnOverlap()
    {
        _movieRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Movie { Id = 1 });
        _roomRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Room { Id = 1 });
        _sessionRepo
            .Setup(r => r.HasOverlapAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateSessionRequest(1, 1,
            new DateTime(2026, 8, 1, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 1, 17, 0, 0, DateTimeKind.Utc),
            35.00m);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _sut.CreateSessionAsync(request));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task CreateSessionAsync_Success_ReturnsSessionAdminDto()
    {
        _movieRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Movie { Id = 1, Title = "Matrix" });
        _roomRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Room { Id = 1, Name = "Sala 1", Rows = 5, Columns = 4 });
        _sessionRepo
            .Setup(r => r.HasOverlapAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
                    .Callback<Session, CancellationToken>((s, _) => s.Id = 10);

        var request = new CreateSessionRequest(1, 1,
            new DateTime(2026, 8, 1, 14, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 1, 17, 0, 0, DateTimeKind.Utc),
            35.00m);

        var result = await _sut.CreateSessionAsync(request);

        Assert.Equal(10, result.Id);
        Assert.Equal(1, result.MovieId);
        Assert.Equal(35.00m, result.TicketPrice);
        Assert.False(result.IsDeleted);
        Assert.NotNull(result.Movie);
        Assert.Equal("Matrix", result.Movie!.Title);
        Assert.NotNull(result.Room);
        Assert.Equal("Sala 1", result.Room!.Name);
    }

    // ── SoftDeleteSessionAsync ─────────────────────────────

    [Fact]
    public async Task SoftDelete_SetsIsDeletedAndCallsUpdate()
    {
        var session = new Session { Id = 1, IsDeleted = false };
        _sessionRepo.Setup(r => r.GetByIdAsync(1, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(session);

        await _sut.SoftDeleteSessionAsync(1);

        Assert.True(session.IsDeleted);
        _sessionRepo.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDelete_ThrowsIfAlreadyDeleted()
    {
        var session = new Session { Id = 1, IsDeleted = true };
        _sessionRepo.Setup(r => r.GetByIdAsync(1, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(session);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _sut.SoftDeleteSessionAsync(1));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task SoftDelete_ThrowsIfSessionNotFound()
    {
        _sessionRepo.Setup(r => r.GetByIdAsync(99, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Session?)null);

        await Assert.ThrowsAsync<SessionNotFoundException>(() => _sut.SoftDeleteSessionAsync(99));
    }

    // ── RestoreSessionAsync ────────────────────────────────

    [Fact]
    public async Task RestoreSession_ClearsIsDeletedAndCallsUpdate()
    {
        var session = new Session
        {
            Id = 1, RoomId = 1, IsDeleted = true,
            StartTime = new DateTime(2026, 8, 1, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 8, 1, 17, 0, 0, DateTimeKind.Utc)
        };
        _sessionRepo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(session);
        _sessionRepo.Setup(r => r.HasOverlapAsync(1,
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.RestoreSessionAsync(1);

        Assert.False(session.IsDeleted);
        _sessionRepo.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreSession_ThrowsIfAlreadyActive()
    {
        var session = new Session { Id = 1, IsDeleted = false };
        _sessionRepo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(session);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _sut.RestoreSessionAsync(1));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task RestoreSession_ThrowsOnOverlap()
    {
        var session = new Session
        {
            Id = 1, RoomId = 1, IsDeleted = true,
            StartTime = new DateTime(2026, 8, 1, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 8, 1, 17, 0, 0, DateTimeKind.Utc)
        };
        _sessionRepo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(session);
        _sessionRepo.Setup(r => r.HasOverlapAsync(1,
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _sut.RestoreSessionAsync(1));
        Assert.Equal(409, ex.StatusCode);
    }
}
