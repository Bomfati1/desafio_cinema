namespace Cinema.Api.Models;

public class Reservation
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }

    // Navigation
    public Session Session { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
