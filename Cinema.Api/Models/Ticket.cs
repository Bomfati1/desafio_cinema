namespace Cinema.Api.Models;

public class Ticket
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int SessionId { get; set; }
    public int SeatId { get; set; }

    // Navigation
    public Reservation Reservation { get; set; } = null!;
    public Session Session { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
}
