using Cinema.Api.Models;

namespace Cinema.Api.DTOs;

public record SessionDetailDto(
    int Id,
    int MovieId,
    int RoomId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TicketPrice,
    MovieDto Movie,
    RoomDto Room,
    List<SeatDto> Seats
);

public record SeatDto(
    int Id,
    int RoomId,
    string Row,
    int Number,
    string Label,
    bool IsOccupied
);

/// <summary>DTO admin — inclui dados da reserva para assentos ocupados.</summary>
public record AdminSeatDto(
    int Id,
    int RoomId,
    string Row,
    int Number,
    string Label,
    bool IsOccupied,
    string? CustomerName,      // null se livre
    DateTime? ReservedAt       // null se livre
);
