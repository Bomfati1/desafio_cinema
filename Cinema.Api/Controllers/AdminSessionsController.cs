using Cinema.Api.DTOs;
using Cinema.Api.Models;
using Cinema.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/admin/sessions")]
[Tags("Admin — Sessions")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminSessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public AdminSessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Lista todas as sessões (inclui desativadas) com paginação, ordenadas por ativas primeiro.
    /// </summary>
    /// <param name="date" example="2026-08-01">Filtrar por data (opcional).</param>
    /// <param name="page">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Itens por página (padrão: 20, máximo: 100).</param>
    /// <response code="200">Lista paginada de sessões (inclui flag IsDeleted).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SessionAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<SessionAdminDto>>> GetAll(
        [FromQuery] DateTime? date = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _sessionService.GetSessionsAdminPagedAsync(date, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Retorna o mapa de assentos da sessão com dados da reserva
    /// (nome do cliente e data/hora) para assentos ocupados.
    /// </summary>
    /// <param name="id">ID da sessão.</param>
    /// <response code="200">Lista de assentos com dados de reserva.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Sessão não encontrada.</response>
    [HttpGet("{id:int}/seats")]
    [ProducesResponseType(typeof(List<AdminSeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AdminSeatDto>>> GetSeats(int id)
    {
        var seats = await _sessionService.GetAdminSessionSeatsAsync(id);
        return Ok(seats);
    }

    /// <summary>
    /// Cria uma nova sessão. Valida conflito de horário na mesma sala.
    /// </summary>
    /// <response code="201">Sessão criada com sucesso.</response>
    /// <response code="400">Dados inválidos (startTime &lt; endTime, ticketPrice &gt; 0).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="409">Conflito de horário com outra sessão na mesma sala.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SessionAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionAdminDto>> Create([FromBody] CreateSessionRequest request)
    {
        var session = await _sessionService.CreateSessionAsync(request);
        return CreatedAtAction(nameof(Create), new { id = session.Id }, session);
    }

    /// <summary>
    /// Desativa uma sessão (soft-delete). A sessão não aparece mais para usuários,
    /// mas permanece no banco e pode ser restaurada.
    /// </summary>
    /// <param name="id">ID da sessão.</param>
    /// <response code="204">Sessão desativada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Sessão não encontrada.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _sessionService.SoftDeleteSessionAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Replica todas as sessões ativas de um dia de origem para um dia de destino,
    /// mantendo filmes, salas e horários. Sessões com conflito de horário são puladas.
    /// </summary>
    /// <param name="request">SourceDate e TargetDate.</param>
    /// <response code="200">Resultado com sessões criadas e erros de conflito.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpPost("replicate")]
    [ProducesResponseType(typeof(ReplicateSessionsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReplicateSessionsResult>> Replicate(
        [FromBody] ReplicateSessionsRequest request)
    {
        var result = await _sessionService.ReplicateSessionsAsync(
            request.SourceDate, request.TargetDate);
        return Ok(result);
    }

    /// <summary>
    /// Restaura uma sessão desativada. Valida conflito de horário com sessões ativas.
    /// </summary>
    /// <param name="id">ID da sessão.</param>
    /// <response code="204">Sessão restaurada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Sessão não encontrada.</response>
    /// <response code="409">Conflito de horário com sessão ativa na mesma sala.</response>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Restore(int id)
    {
        await _sessionService.RestoreSessionAsync(id);
        return NoContent();
    }
}
