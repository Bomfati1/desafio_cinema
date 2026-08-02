using Cinema.Api.DTOs;
using Cinema.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Reservations")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>
    /// Cria uma reserva de assentos para uma sessão (requer autenticação).
    /// A operação é transacional — se qualquer assento já estiver ocupado, a reserva é rejeitada.
    /// </summary>
    /// <response code="201">Reserva criada com sucesso.</response>
    /// <response code="400">Dados inválidos (campos obrigatórios, mínimo 1 assento).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="404">Sessão ou assento não encontrado.</response>
    /// <response code="409">Conflito — um ou mais assentos já estão ocupados.</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> CreateReservation(
        [FromBody] CreateReservationRequest request)
    {
        var reservation = await _reservationService.CreateReservationAsync(request);
        return CreatedAtAction(nameof(CreateReservation), new { id = reservation.Id }, reservation);
    }
}
