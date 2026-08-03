using Cinema.Api.DTOs;
using Cinema.Api.Models;

namespace Cinema.Api.Services;

public interface ISessionService
{
    Task<List<SessionDto>> GetSessionsAsync(DateTime? date = null);
    Task<PagedResult<SessionDto>> GetSessionsPagedAsync(DateTime? date = null, int page = 1, int pageSize = 20);
    Task<PagedResult<SessionAdminDto>> GetSessionsAdminPagedAsync(DateTime? date = null, int page = 1, int pageSize = 20);
    Task<SessionDetailDto?> GetSessionDetailAsync(int sessionId);
    Task<List<AdminSeatDto>> GetAdminSessionSeatsAsync(int sessionId);
    Task<SessionAdminDto> CreateSessionAsync(CreateSessionRequest request);
    Task<ReplicateSessionsResult> ReplicateSessionsAsync(DateTime sourceDate, DateTime targetDate);
    Task SoftDeleteSessionAsync(int sessionId);
    Task RestoreSessionAsync(int sessionId);
}
