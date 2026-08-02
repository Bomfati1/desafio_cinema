using Cinema.Api.DTOs;
using Cinema.Api.Models;
using Cinema.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Sessions")]
[Produces("application/json")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Lista as sessões disponíveis com paginação (exclui sessões desativadas).
    /// </summary>
    /// <param name="date" example="2026-08-01">Filtrar por data (opcional).</param>
    /// <param name="page">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Itens por página (padrão: 20, máximo: 100).</param>
    /// <response code="200">Lista paginada de sessões ativas.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SessionDto>>> GetSessions(
        [FromQuery] DateTime? date = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _sessionService.GetSessionsPagedAsync(date, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Obtém detalhes de uma sessão com o mapa completo de assentos (livres/ocupados).
    /// </summary>
    /// <param name="id">ID da sessão.</param>
    /// <response code="200">Detalhes da sessão com assentos.</response>
    /// <response code="404">Sessão não encontrada.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SessionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionDetailDto>> GetSession(int id)
    {
        var session = await _sessionService.GetSessionDetailAsync(id);
        return Ok(session);
    }
}
