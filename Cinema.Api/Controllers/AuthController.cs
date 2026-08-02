using Cinema.Api.DTOs;
using Cinema.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT + refresh token.
    /// Use as credenciais de seed para teste:
    /// Admin: admin@cinema.com / admin
    /// User: user@email.com / user
    /// </summary>
    /// <response code="200">Login realizado com sucesso. Retorna token JWT e refresh token.</response>
    /// <response code="401">Email ou senha inválidos.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Renova o token JWT usando um refresh token válido.
    /// O refresh token é rotacionado — cada token só pode ser usado uma vez.
    /// </summary>
    /// <response code="200">Token renovado com sucesso. Retorna novo par token + refresh token.</response>
    /// <response code="401">Refresh token inválido, expirado ou já revogado.</response>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(response);
    }
}
