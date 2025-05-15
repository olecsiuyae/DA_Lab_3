using Microsoft.Extensions.Logging;
using ReservationService.Models;

namespace ReservationService.Repositories;

/// <inheritdoc/>
public class InMemoryReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _reservations = new List<Reservation>();
    private readonly ILogger<InMemoryReservationRepository> _logger;
    private readonly object _lock = new();
    private int _lastId = 1000;

    public InMemoryReservationRepository(ILogger<InMemoryReservationRepository> logger)
    {
        _logger = logger;
        _logger.LogInformation("{Method} flow.", "InMemoryReservationRepository constructor");
    }

    public Reservation CreateReservation(Reservation reservation)
    {
        _logger.LogInformation("{Method} flow. CustomerId: {CustomerId}, TicketId: {TicketId}",
            "CreateReservation", reservation.CustomerId, reservation.TicketId);
        
        lock (_lock)
        {
            // Generate a new ID
            reservation.Id = $"R{++_lastId}";
            reservation.CreatedAt = DateTime.UtcNow.ToString("o");
            
            _reservations.Add(reservation);
            
            _logger.LogInformation("Reservation created with ID {ReservationId}", reservation.Id);
            
            return reservation;
        }
    }

    public Reservation? GetReservationById(string reservationId)
    {
        _logger.LogInformation("{Method} flow. ReservationId: {ReservationId}", 
            "GetReservationById", reservationId);
        
        return _reservations.FirstOrDefault(r => r.Id == reservationId);
    }

    public List<Reservation> GetReservationsByCustomerId(string customerId)
    {
        _logger.LogInformation("{Method} flow. CustomerId: {CustomerId}", 
            "GetReservationsByCustomerId", customerId);
        
        return _reservations.Where(r => r.CustomerId == customerId).ToList();
    }

    public bool UpdateReservationStatus(string reservationId, string status)
    {
        _logger.LogInformation("{Method} flow. ReservationId: {ReservationId}, Status: {Status}", 
            "UpdateReservationStatus", reservationId, status);
        
        lock (_lock)
        {
            var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
            
            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found", reservationId);
                return false;
            }

            reservation.Status = status;
            
            _logger.LogInformation("Reservation with ID {ReservationId} status updated to {Status}", 
                reservationId, status);
            
            return true;
        }
    }
} 