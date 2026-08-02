namespace Cinema.Api.DTOs;

public record LoginResponse(
    string Token,
    string RefreshToken,
    string Email,
    string Name,
    string Role);

public record RefreshTokenRequest(string RefreshToken);
