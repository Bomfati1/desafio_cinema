using Cinema.Api.DTOs;
using Cinema.Api.Exceptions;
using Cinema.Api.Models;
using Cinema.Api.Repositories;

namespace Cinema.Api.Services;

public interface IMovieService
{
    Task<MovieDto> CreateMovieAsync(CreateMovieRequest request);
    Task<List<MovieDto>> GetAllMoviesAsync();
    Task DeleteMovieAsync(int movieId);
}

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repo;

    public MovieService(IMovieRepository repo) => _repo = repo;

    public async Task<MovieDto> CreateMovieAsync(CreateMovieRequest request)
    {
        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            Genre = request.Genre,
            DurationMinutes = request.DurationMinutes,
            PosterUrl = request.PosterUrl
        };

        await _repo.AddAsync(movie);
        return MapToDto(movie);
    }

    public async Task<List<MovieDto>> GetAllMoviesAsync()
    {
        var movies = await _repo.GetAllAsync();
        return movies.Select(MapToDto).ToList();
    }

    public async Task DeleteMovieAsync(int movieId)
    {
        var movie = await _repo.GetByIdAsync(movieId)
            ?? throw new MovieNotFoundException(movieId);

        await _repo.DeleteAsync(movie);
    }

    private static MovieDto MapToDto(Movie m) => new(
        m.Id, m.Title, m.Description, m.Genre, m.DurationMinutes, m.PosterUrl);
}
