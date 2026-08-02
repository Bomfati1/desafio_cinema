namespace Cinema.Api.DTOs;

public record CreateMovieRequest(
    string Title,
    string Description,
    string Genre,
    int DurationMinutes,
    string PosterUrl
);
