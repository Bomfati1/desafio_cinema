using Cinema.Api.DTOs;

namespace Cinema.Api.Services;

public interface IReservationService
{
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request);
}
