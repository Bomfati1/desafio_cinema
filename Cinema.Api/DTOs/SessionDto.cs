namespace Cinema.Api.DTOs;

/// <summary>DTO público — exibido para usuários (sem campos internos).</summary>
public record SessionDto(
    int Id,
    int MovieId,
    int RoomId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TicketPrice,
    MovieDto? Movie,
    RoomDto? Room
);

/// <summary>DTO admin — inclui flag de soft-delete para o painel.</summary>
public record SessionAdminDto(
    int Id,
    int MovieId,
    int RoomId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TicketPrice,
    MovieDto? Movie,
    RoomDto? Room,
    bool IsDeleted
);

public record MovieDto(
    int Id,
    string Title,
    string Description,
    string Genre,
    int DurationMinutes,
    string PosterUrl,
    bool IsDeleted = false
);

public record RoomDto(
    int Id,
    string Name,
    int Rows,
    int Columns,
    bool IsDeleted = false
);
