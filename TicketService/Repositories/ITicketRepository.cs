using TicketService.Models;

namespace TicketService.Repositories;

/// <summary>
/// Interface for ticket repository operations
/// </summary>
public interface ITicketRepository
{
    /// <summary>
    /// Get all available tickets
    /// </summary>
    /// <returns>List of available tickets</returns>
    List<Ticket> GetAllTickets();
    
    /// <summary>
    /// Get ticket by ID
    /// </summary>
    /// <param name="ticketId">ID of the ticket to retrieve</param>
    /// <returns>Ticket if found, null otherwise</returns>
    Ticket? GetTicketById(string ticketId);
    
    /// <summary>
    /// Check if a ticket is available
    /// </summary>
    /// <param name="ticketId">ID of the ticket to check</param>
    /// <returns>True if available, false otherwise</returns>
    bool IsTicketAvailable(string ticketId);
    
    /// <summary>
    /// Reserve a ticket for a customer
    /// </summary>
    /// <param name="ticketId">ID of the ticket to reserve</param>
    /// <param name="reservationId">ID of the reservation</param>
    /// <param name="customerId">ID of the customer</param>
    /// <returns>True if reservation successful, false otherwise</returns>
    bool ReserveTicket(string ticketId, string reservationId, string customerId);
    
    /// <summary>
    /// Release a previously reserved ticket
    /// </summary>
    /// <param name="ticketId">ID of the ticket to release</param>
    /// <param name="reservationId">ID of the reservation</param>
    /// <returns>True if release successful, false otherwise</returns>
    bool ReleaseTicket(string ticketId, string reservationId);
} 