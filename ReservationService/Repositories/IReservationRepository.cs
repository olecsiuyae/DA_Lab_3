using ReservationService.Models;

namespace ReservationService.Repositories;

/// <summary>
/// Interface for reservation repository operations
/// </summary>
public interface IReservationRepository
{
    /// <summary>
    /// Create a new reservation
    /// </summary>
    /// <param name="reservation">The reservation to create</param>
    /// <returns>The created reservation</returns>
    Reservation CreateReservation(Reservation reservation);
    
    /// <summary>
    /// Get a reservation by ID
    /// </summary>
    /// <param name="reservationId">ID of the reservation to retrieve</param>
    /// <returns>Reservation if found, null otherwise</returns>
    Reservation? GetReservationById(string reservationId);
    
    /// <summary>
    /// Get all reservations for a customer
    /// </summary>
    /// <param name="customerId">ID of the customer</param>
    /// <returns>List of customer's reservations</returns>
    List<Reservation> GetReservationsByCustomerId(string customerId);
    
    /// <summary>
    /// Update the status of a reservation
    /// </summary>
    /// <param name="reservationId">ID of the reservation to update</param>
    /// <param name="status">New status</param>
    /// <returns>True if update successful, false otherwise</returns>
    bool UpdateReservationStatus(string reservationId, string status);
} 