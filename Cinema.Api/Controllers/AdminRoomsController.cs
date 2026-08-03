using Cinema.Api.DTOs;
using Cinema.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/admin/rooms")]
[Tags("Admin — Rooms")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminRoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public AdminRoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Lista todas as salas cadastradas com suas dimensões.
    /// </summary>
    /// <response code="200">Lista de salas.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RoomDto>>> GetAll()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    /// <summary>
    /// Cria uma nova sala com geração automática de assentos (Rows × Columns).
    /// Fileiras nomeadas de A a Z (máximo 26 fileiras).
    /// </summary>
    /// <response code="201">Sala criada com assentos gerados.</response>
    /// <response code="400">Dados inválidos (nome obrigatório, rows &gt; 0, columns &gt; 0).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpPost]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RoomDto>> Create([FromBody] CreateRoomRequest request)
    {
        var room = await _roomService.CreateRoomAsync(request);
        return CreatedAtAction(nameof(Create), new { id = room.Id }, room);
    }

    /// <summary>
    /// Desativa uma sala (soft-delete). A sala não aparece mais para seleção,
    /// mas seus dados e sessões são preservados. Pode ser restaurada.
    /// </summary>
    /// <param name="id">ID da sala.</param>
    /// <response code="204">Sala desativada com sucesso.</response>
    /// <response code="400">Sala já está desativada.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Sala não encontrada.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _roomService.SoftDeleteRoomAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Restaura uma sala desativada. A sala volta a ficar disponível para seleção.
    /// </summary>
    /// <param name="id">ID da sala.</param>
    /// <response code="204">Sala restaurada com sucesso.</response>
    /// <response code="400">Sala já está ativa.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Sala não encontrada.</response>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(int id)
    {
        await _roomService.RestoreRoomAsync(id);
        return NoContent();
    }
}
