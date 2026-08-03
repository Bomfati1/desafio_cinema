using Cinema.Api.DTOs;
using Cinema.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/admin/movies")]
[Tags("Admin — Movies")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminMoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public AdminMoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    /// <summary>
    /// Lista todos os filmes cadastrados no catálogo.
    /// </summary>
    /// <response code="200">Lista de filmes.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<MovieDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<MovieDto>>> GetAll()
    {
        var movies = await _movieService.GetAllMoviesAsync();
        return Ok(movies);
    }

    /// <summary>
    /// Cadastra um novo filme no catálogo.
    /// </summary>
    /// <response code="201">Filme criado com sucesso.</response>
    /// <response code="400">Dados inválidos (título obrigatório, duração &gt; 0).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpPost]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MovieDto>> Create([FromBody] CreateMovieRequest request)
    {
        var movie = await _movieService.CreateMovieAsync(request);
        return CreatedAtAction(nameof(Create), new { id = movie.Id }, movie);
    }

    /// <summary>
    /// Atualiza os dados de um filme existente.
    /// </summary>
    /// <param name="id">ID do filme.</param>
    /// <response code="200">Filme atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Filme não encontrado.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDto>> Update(int id, [FromBody] CreateMovieRequest request)
    {
        var movie = await _movieService.UpdateMovieAsync(id, request);
        return Ok(movie);
    }

    /// <summary>
    /// Exclui permanentemente um filme e todas as suas sessões associadas (hard delete).
    /// </summary>
    /// <param name="id">ID do filme.</param>
    /// <response code="204">Filme excluído com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Filme não encontrado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _movieService.DeleteMovieAsync(id);
        return NoContent();
    }
}
