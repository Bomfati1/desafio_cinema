namespace Cinema.Api.Models;

public class Seat
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string Row { get; set; } = string.Empty;   // "A", "B", "C"...
    public int Number { get; set; }                    // 1, 2, 3...
    public string Label => $"{Row}{Number}";           // "A1", "B3"...

    // Navigation
    public Room Room { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
