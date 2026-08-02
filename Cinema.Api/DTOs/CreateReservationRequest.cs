namespace Cinema.Api.DTOs;

public record CreateReservationRequest(
    int SessionId,
    string CustomerName,
    List<int> SeatIds
);
