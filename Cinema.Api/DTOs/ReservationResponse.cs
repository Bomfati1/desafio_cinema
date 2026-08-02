using Cinema.Api.Models;

namespace Cinema.Api.DTOs;

public record ReservationResponse(
    int Id,
    int SessionId,
    string CustomerName,
    DateTime ReservedAt,
    List<TicketDto> Tickets
);

public record TicketDto(
    int Id,
    int ReservationId,
    int SessionId,
    int SeatId,
    SeatDto? Seat
);
