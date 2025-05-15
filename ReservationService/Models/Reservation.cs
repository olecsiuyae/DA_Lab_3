namespace ReservationService.Models;

public class Reservation
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventDate { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public double Price { get; set; }
    public string Status { get; set; } = "Reserved"; // "Reserved", "Paid", "Cancelled"
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
} 