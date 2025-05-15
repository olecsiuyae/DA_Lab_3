namespace TicketService.Models;

public class Ticket
{
    public string Id { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventDate { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public double Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string Section { get; set; } = string.Empty;
    public string Row { get; set; } = string.Empty;
    public string Seat { get; set; } = string.Empty;
    public string? ReservationId { get; set; }
    public string? CustomerId { get; set; }
} 