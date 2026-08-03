namespace Cinema.Api.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rows { get; set; }
    public int Columns { get; set; }
    public bool IsDeleted { get; set; } = false; // soft-delete

    // Navigation
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
