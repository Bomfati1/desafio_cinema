using Cinema.Api.DTOs;
using Cinema.Api.Exceptions;
using Cinema.Api.Models;
using Cinema.Api.Repositories;

namespace Cinema.Api.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repo;
    private readonly IMovieRepository _movieRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IReservationRepository _reservationRepo;

    public SessionService(
        ISessionRepository repo,
        IMovieRepository movieRepo,
        IRoomRepository roomRepo,
        IReservationRepository reservationRepo)
    {
        _repo = repo;
        _movieRepo = movieRepo;
        _roomRepo = roomRepo;
        _reservationRepo = reservationRepo;
    }

    public async Task<List<SessionDto>> GetSessionsAsync(DateTime? date = null)
    {
        var sessions = await _repo.GetSessionsAsync(date, includeDeleted: false);
        return sessions.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<SessionDto>> GetSessionsPagedAsync(DateTime? date = null, int page = 1, int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, PaginationDefaults.MaxPageSize);
        var (items, totalCount) = await _repo.GetSessionsPagedAsync(date, includeDeleted: false, page, pageSize);

        return new PagedResult<SessionDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<SessionAdminDto>> GetSessionsAdminPagedAsync(DateTime? date = null, int page = 1, int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, PaginationDefaults.MaxPageSize);
        var (items, totalCount) = await _repo.GetSessionsPagedAsync(date, includeDeleted: true, page, pageSize);

        return new PagedResult<SessionAdminDto>
        {
            Items = items
                .OrderBy(s => s.IsDeleted)
                .ThenBy(s => s.Room?.Name)
                .ThenBy(s => s.StartTime)
                .Select(MapToAdminDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SessionDetailDto?> GetSessionDetailAsync(int sessionId)
    {
        var session = await _repo.GetByIdWithDetailsAsync(sessionId);
        if (session is null)
            throw new SessionNotFoundException(sessionId);

        var occupiedSeatIds = await _reservationRepo.GetOccupiedSeatIdsAsync(sessionId);

        var allSeats = await _roomRepo.GetSeatsByRoomAsync(session.RoomId);

        var seatDtos = allSeats
            .Select(s => new SeatDto(
                s.Id, s.RoomId, s.Row, s.Number, s.Label,
                occupiedSeatIds.Contains(s.Id)))
            .ToList();

        return new SessionDetailDto(
            session.Id, session.MovieId, session.RoomId,
            session.StartTime, session.EndTime, session.TicketPrice,
            MapMovieDto(session.Movie),
            new RoomDto(session.Room.Id, session.Room.Name, session.Room.Rows, session.Room.Columns),
            seatDtos);
    }

    public async Task<List<AdminSeatDto>> GetAdminSessionSeatsAsync(int sessionId)
    {
        var session = await _repo.GetByIdWithDetailsAsync(sessionId);
        if (session is null)
            throw new SessionNotFoundException(sessionId);

        // Busca tickets com dados da reserva (CustomerName, ReservedAt)
        var tickets = await _reservationRepo.GetTicketsWithReservationsAsync(sessionId);

        // Dicionário: seatId → (customerName, reservedAt) para lookup rápido
        var reservationBySeat = tickets
            .Where(t => t.Reservation != null)
            .ToDictionary(t => t.SeatId, t => (t.Reservation!.CustomerName, t.Reservation!.ReservedAt));

        var allSeats = await _roomRepo.GetSeatsByRoomAsync(session.RoomId);

        return allSeats
            .Select(s =>
            {
                var hasReservation = reservationBySeat.TryGetValue(s.Id, out var res);
                return new AdminSeatDto(
                    s.Id, s.RoomId, s.Row, s.Number, s.Label,
                    hasReservation,
                    hasReservation ? res.CustomerName : null,
                    hasReservation ? res.ReservedAt : null);
            })
            .ToList();
    }

    public async Task<SessionAdminDto> CreateSessionAsync(CreateSessionRequest request)
    {
        var movie = await _movieRepo.GetByIdAsync(request.MovieId)
            ?? throw new MovieNotFoundException(request.MovieId);

        var room = await _roomRepo.GetByIdAsync(request.RoomId)
            ?? throw new RoomNotFoundException(request.RoomId);

        var startTime = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
        var endTime = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);

        // Validações de integridade já cobertas pelo CreateSessionValidator (FluentValidation)
        // Aqui apenas as regras de negócio que dependem de estado do banco

        if (await _repo.HasOverlapAsync(request.RoomId, startTime, endTime))
            throw new DomainException(
                "Conflito de horário: já existe uma sessão nesta sala durante o período informado.", 409);

        var session = new Session
        {
            MovieId = request.MovieId,
            RoomId = request.RoomId,
            StartTime = startTime,
            EndTime = endTime,
            TicketPrice = request.TicketPrice
        };

        await _repo.AddAsync(session);

        return new SessionAdminDto(
            session.Id, session.MovieId, session.RoomId,
            session.StartTime, session.EndTime, session.TicketPrice,
            MapMovieDto(movie),
            new RoomDto(room.Id, room.Name, room.Rows, room.Columns),
            session.IsDeleted);
    }

    public async Task SoftDeleteSessionAsync(int sessionId)
    {
        var session = await _repo.GetByIdAsync(sessionId)
            ?? throw new SessionNotFoundException(sessionId);

        if (session.IsDeleted)
            throw new DomainException("Esta sessão já está desativada.", 400);

        session.IsDeleted = true;
        await _repo.UpdateAsync(session);
    }

    public async Task RestoreSessionAsync(int sessionId)
    {
        var session = await _repo.GetByIdAsync(sessionId, includeDeleted: true)
            ?? throw new SessionNotFoundException(sessionId);

        if (!session.IsDeleted)
            throw new DomainException("Esta sessão já está ativa.", 400);

        if (await _repo.HasOverlapAsync(session.RoomId, session.StartTime, session.EndTime, excludeId: sessionId))
            throw new DomainException(
                "Conflito de horário: não é possível restaurar esta sessão pois há outra sessão ativa na mesma sala durante esse período.", 409);

        session.IsDeleted = false;
        await _repo.UpdateAsync(session);
    }

    // ── Helpers ─────────────────────────────────────────────

    private static SessionDto MapToDto(Session s) => new(
        s.Id, s.MovieId, s.RoomId, s.StartTime, s.EndTime, s.TicketPrice,
        s.Movie != null ? MapMovieDto(s.Movie) : null,
        s.Room != null ? new RoomDto(s.Room.Id, s.Room.Name, s.Room.Rows, s.Room.Columns) : null);

    private static SessionAdminDto MapToAdminDto(Session s) => new(
        s.Id, s.MovieId, s.RoomId, s.StartTime, s.EndTime, s.TicketPrice,
        s.Movie != null ? MapMovieDto(s.Movie) : null,
        s.Room != null ? new RoomDto(s.Room.Id, s.Room.Name, s.Room.Rows, s.Room.Columns) : null,
        s.IsDeleted);

    private static MovieDto MapMovieDto(Movie m) => new(
        m.Id, m.Title, m.Description, m.Genre, m.DurationMinutes, m.PosterUrl);
}
