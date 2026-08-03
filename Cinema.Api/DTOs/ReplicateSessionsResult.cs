namespace Cinema.Api.DTOs;

public record ReplicateSessionsResult(
    int CreatedCount,
    int SkippedCount,
    List<SessionAdminDto> CreatedSessions,
    List<string> Errors
);
