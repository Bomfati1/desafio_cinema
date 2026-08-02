namespace Cinema.Api.Models;

/// <summary>
/// Token de atualização (refresh token) para renovar JWT sem re-login.
/// Armazenado no banco para permitir revogação server-side.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }

    public User? User { get; set; }
}
