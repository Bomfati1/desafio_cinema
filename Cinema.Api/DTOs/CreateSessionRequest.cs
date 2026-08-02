namespace Cinema.Api.DTOs;

public record CreateSessionRequest(
    int MovieId,
    int RoomId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TicketPrice
);
