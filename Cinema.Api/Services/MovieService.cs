using Cinema.Api.DTOs;
using Cinema.Api.Exceptions;
using Cinema.Api.Models;
using Cinema.Api.Repositories;

namespace Cinema.Api.Services;

public interface IMovieService
{
    Task<MovieDto> CreateMovieAsync(CreateMovieRequest request);
    Task<MovieDto> UpdateMovieAsync(int movieId, CreateMovieRequest request);
    Task<List<MovieDto>> GetAllMoviesAsync();
    Task SoftDeleteMovieAsync(int movieId);
    Task RestoreMovieAsync(int movieId);
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

    public async Task<MovieDto> UpdateMovieAsync(int movieId, CreateMovieRequest request)
    {
        var movie = await _repo.GetByIdAsync(movieId)
            ?? throw new MovieNotFoundException(movieId);

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.Genre = request.Genre;
        movie.DurationMinutes = request.DurationMinutes;
        movie.PosterUrl = request.PosterUrl;

        await _repo.UpdateAsync(movie);
        return MapToDto(movie);
    }

    public async Task<List<MovieDto>> GetAllMoviesAsync()
    {
        var movies = await _repo.GetAllAsync(includeDeleted: true);
        return movies.Select(MapToDto).ToList();
    }

    public async Task SoftDeleteMovieAsync(int movieId)
    {
        var movie = await _repo.GetByIdAsync(movieId)
            ?? throw new MovieNotFoundException(movieId);

        if (movie.IsDeleted)
            throw new DomainException("Este filme já está desativado.", 400);

        movie.IsDeleted = true;
        await _repo.UpdateAsync(movie);
    }

    public async Task RestoreMovieAsync(int movieId)
    {
        var movie = await _repo.GetByIdAsync(movieId, includeDeleted: true)
            ?? throw new MovieNotFoundException(movieId);

        if (!movie.IsDeleted)
            throw new DomainException("Este filme já está ativo.", 400);

        movie.IsDeleted = false;
        await _repo.UpdateAsync(movie);
    }

    private static MovieDto MapToDto(Movie m) => new(
        m.Id, m.Title, m.Description, m.Genre, m.DurationMinutes, m.PosterUrl, m.IsDeleted);
}
