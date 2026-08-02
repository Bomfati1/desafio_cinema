using Cinema.Api.Data;
using Cinema.Api.DTOs;
using Cinema.Api.Exceptions;
using Cinema.Api.Repositories;

namespace Cinema.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepo, IUnitOfWork uow, ITokenService tokenService)
    {
        _userRepo = userRepo;
        _uow = uow;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);

        if (user is null)
            throw new InvalidCredentialsException();

        if (!await VerifyAndUpgradeHash(user, request.Password))
            throw new InvalidCredentialsException();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

        return new LoginResponse(accessToken, refreshToken, user.Email, user.Name, user.Role);
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _tokenService.ValidateRefreshTokenAsync(refreshToken)
            ?? throw new InvalidCredentialsException();

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

        return new LoginResponse(newAccessToken, newRefreshToken, user.Email, user.Name, user.Role);
    }

    /// <summary>
    /// Verifica a senha e faz upgrade automático de SHA256 → BCrypt quando necessário.
    /// </summary>
    private async Task<bool> VerifyAndUpgradeHash(Models.User user, string password)
    {
        // Hash BCrypt (formato: $2a$...)
        if (user.PasswordHash.StartsWith("$2"))
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        // Hash SHA256 legado (64 caracteres hex) — migração transparente
        if (user.PasswordHash.Length == 64)
        {
            var legacyHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(password)));

            if (string.Equals(user.PasswordHash, legacyHash, StringComparison.OrdinalIgnoreCase))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
                await _uow.SaveChangesAsync();
                return true;
            }

            return false;
        }

        return false;
    }

    public static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}
