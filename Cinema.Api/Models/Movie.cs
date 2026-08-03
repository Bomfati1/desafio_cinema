namespace Cinema.Api.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false; // soft-delete

    // Navigation
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
