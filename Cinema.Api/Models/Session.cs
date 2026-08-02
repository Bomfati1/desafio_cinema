namespace Cinema.Api.Models;

public class Session
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TicketPrice { get; set; }
    public bool IsDeleted { get; set; } = false; // soft-delete

    // Navigation
    public Movie Movie { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
