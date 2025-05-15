using Microsoft.Extensions.Logging;
using TicketService.Models;

namespace TicketService.Repositories;

/// <inheritdoc/>
public class InMemoryTicketRepository : ITicketRepository
{
    private readonly List<Ticket> _tickets;
    private readonly ILogger<InMemoryTicketRepository> _logger;
    private readonly object _lock = new();

    public InMemoryTicketRepository(ILogger<InMemoryTicketRepository> logger)
    {
        _logger = logger;
        _logger.LogInformation("{Method} flow.", "InMemoryTicketRepository constructor");
        
        // Initialize with some sample tickets
        _tickets = [
            new Ticket
            {
                Id = "T1001",
                EventName = "Rock Concert",
                EventDate = "2025-04-15T19:00:00",
                Venue = "City Arena",
                Price = 85.50,
                Section = "A",
                Row = "1",
                Seat = "12"
            },
            new Ticket
            {
                Id = "T1002",
                EventName = "Rock Concert",
                EventDate = "2025-04-15T19:00:00",
                Venue = "City Arena",
                Price = 75.00,
                Section = "B",
                Row = "3",
                Seat = "5"
            },
            new Ticket
            {
                Id = "T1003",
                EventName = "Classical Symphony",
                EventDate = "2025-04-20T18:30:00",
                Venue = "Concert Hall",
                Price = 120.00,
                Section = "Premium",
                Row = "2",
                Seat = "7"
            },
            new Ticket
            {
                Id = "T1004",
                EventName = "Basketball Game",
                EventDate = "2025-04-22T20:00:00",
                Venue = "Sports Center",
                Price = 65.00,
                Section = "Lower",
                Row = "10",
                Seat = "15"
            },
            new Ticket
            {
                Id = "T1005",
                EventName = "Theater Play",
                EventDate = "2025-04-25T19:30:00",
                Venue = "City Theater",
                Price = 95.00,
                Section = "Orchestra",
                Row = "5",
                Seat = "8"
            }
        ];
    }

    public List<Ticket> GetAllTickets()
    {
        _logger.LogInformation("{Method} flow.", "GetAllTickets");
        return _tickets;
    }

    public Ticket? GetTicketById(string ticketId)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}", "GetTicketById", ticketId);
        
        return _tickets.FirstOrDefault(t => t.Id == ticketId);
    }

    public bool IsTicketAvailable(string ticketId)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}", "IsTicketAvailable", ticketId);
        
        var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
        
        if (ticket == null)
        {
            _logger.LogWarning("Ticket with ID {TicketId} not found", ticketId);
            return false;
        }

        return ticket.IsAvailable;
    }

    public bool ReserveTicket(string ticketId, string reservationId, string customerId)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}, ReservationId: {ReservationId}, CustomerId: {CustomerId}", 
            "ReserveTicket", ticketId, reservationId, customerId);
        
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            
            if (ticket == null)
            {
                _logger.LogWarning("Ticket with ID {TicketId} not found", ticketId);
                return false;
            }

            if (!ticket.IsAvailable)
            {
                _logger.LogWarning("Ticket with ID {TicketId} is not available", ticketId);
                return false;
            }

            ticket.IsAvailable = false;
            ticket.ReservationId = reservationId;
            ticket.CustomerId = customerId;
            
            _logger.LogInformation("Ticket with ID {TicketId} successfully reserved with reservation ID {ReservationId}", 
                ticketId, reservationId);
            
            return true;
        }
    }

    public bool ReleaseTicket(string ticketId, string reservationId)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}, ReservationId: {ReservationId}", 
            "ReleaseTicket", ticketId, reservationId);
        
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            
            if (ticket == null)
            {
                _logger.LogWarning("Ticket with ID {TicketId} not found", ticketId);
                return false;
            }

            if (ticket.IsAvailable)
            {
                _logger.LogWarning("Ticket with ID {TicketId} is already available", ticketId);
                return false;
            }

            if (ticket.ReservationId != reservationId)
            {
                _logger.LogWarning("Ticket with ID {TicketId} is reserved with a different reservation ID", ticketId);
                return false;
            }

            ticket.IsAvailable = true;
            ticket.ReservationId = null;
            ticket.CustomerId = null;
            
            _logger.LogInformation("Ticket with ID {TicketId} successfully released", ticketId);
            
            return true;
        }
    }
} 