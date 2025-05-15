using Grpc.Core;
using Microsoft.Extensions.Logging;
using ReservationService.Models;
using ReservationService.Protos;
using ReservationService.Repositories;

namespace ReservationService.Services;

public class ReservationManagerService : ReservationManager.ReservationManagerBase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ITicketServiceClient _ticketServiceClient;
    private readonly ILogger<ReservationManagerService> _logger;

    public ReservationManagerService(
        IReservationRepository reservationRepository,
        ITicketServiceClient ticketServiceClient,
        ILogger<ReservationManagerService> logger)
    {
        _reservationRepository = reservationRepository;
        _ticketServiceClient = ticketServiceClient;
        _logger = logger;
        _logger.LogInformation("{Method} flow.", "ReservationManagerService constructor");
    }

    public override async Task<ReservationResponse> CreateReservation(CreateReservationRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow. CustomerId: {CustomerId}, TicketId: {TicketId}", 
            "CreateReservation", request.CustomerId, request.TicketId);
        
        // Check if the ticket is available
        var isAvailable = await _ticketServiceClient.CheckAvailabilityAsync(request.TicketId);
        
        if (!isAvailable)
        {
            _logger.LogWarning("Ticket {TicketId} is not available", request.TicketId);
            return new ReservationResponse
            {
                Success = false,
                Message = $"Ticket {request.TicketId} is not available"
            };
        }
        
        try
        {
            // Get ticket details
            var ticketDetails = await _ticketServiceClient.GetTicketByIdAsync(request.TicketId);
            
            // Create a new reservation
            var reservation = new Reservation
            {
                CustomerId = request.CustomerId,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                TicketId = request.TicketId,
                EventName = ticketDetails.EventName,
                EventDate = ticketDetails.EventDate,
                Venue = ticketDetails.Venue,
                Price = ticketDetails.Price,
                Status = "Reserved"
            };
            
            // Save the reservation
            var createdReservation = _reservationRepository.CreateReservation(reservation);
            
            // Reserve the ticket in the ticket service
            var (success, message) = await _ticketServiceClient.ReserveTicketAsync(
                request.TicketId, 
                createdReservation.Id, 
                request.CustomerId);
            
            if (!success)
            {
                _logger.LogWarning("Failed to reserve ticket {TicketId}: {Message}", request.TicketId, message);
                // Update reservation status to failed
                _reservationRepository.UpdateReservationStatus(createdReservation.Id, "Failed");
                
                return new ReservationResponse
                {
                    Success = false,
                    Message = $"Failed to reserve ticket: {message}"
                };
            }
            
            // Create the response
            var response = new ReservationResponse
            {
                Success = true,
                Message = "Reservation created successfully",
                ReservationId = createdReservation.Id,
                Reservation = MapToReservationDetails(createdReservation)
            };
            
            _logger.LogInformation("Reservation created successfully with ID {ReservationId}", createdReservation.Id);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation for ticket {TicketId}", request.TicketId);
            
            return new ReservationResponse
            {
                Success = false,
                Message = $"Error creating reservation: {ex.Message}"
            };
        }
    }

    public override Task<ReservationDetails> GetReservation(GetReservationRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow. ReservationId: {ReservationId}", 
            "GetReservation", request.ReservationId);
        
        var reservation = _reservationRepository.GetReservationById(request.ReservationId);
        
        if (reservation == null)
        {
            _logger.LogWarning("Reservation with ID {ReservationId} not found", request.ReservationId);
            throw new RpcException(new Status(StatusCode.NotFound, $"Reservation with ID {request.ReservationId} not found"));
        }
        
        var response = MapToReservationDetails(reservation);
        
        _logger.LogInformation("GetReservation successful for reservation ID {ReservationId}", request.ReservationId);
        
        return Task.FromResult(response);
    }

    public override Task<CustomerReservationsResponse> GetCustomerReservations(GetCustomerReservationsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow. CustomerId: {CustomerId}", 
            "GetCustomerReservations", request.CustomerId);
        
        var reservations = _reservationRepository.GetReservationsByCustomerId(request.CustomerId);
        var response = new CustomerReservationsResponse();
        
        foreach (var reservation in reservations)
        {
            response.Reservations.Add(MapToReservationDetails(reservation));
        }
        
        _logger.LogInformation("GetCustomerReservations returning {Count} reservations for customer {CustomerId}", 
            response.Reservations.Count, request.CustomerId);
        
        return Task.FromResult(response);
    }

    public override async Task<CancelReservationResponse> CancelReservation(CancelReservationRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow. ReservationId: {ReservationId}", 
            "CancelReservation", request.ReservationId);
        
        var reservation = _reservationRepository.GetReservationById(request.ReservationId);
        
        if (reservation == null)
        {
            _logger.LogWarning("Reservation with ID {ReservationId} not found", request.ReservationId);
            return new CancelReservationResponse
            {
                Success = false,
                Message = $"Reservation with ID {request.ReservationId} not found"
            };
        }
        
        if (reservation.Status == "Cancelled")
        {
            _logger.LogWarning("Reservation with ID {ReservationId} is already cancelled", request.ReservationId);
            return new CancelReservationResponse
            {
                Success = false,
                Message = "Reservation is already cancelled"
            };
        }
        
        // Release the ticket in the ticket service
        var (success, message) = await _ticketServiceClient.ReleaseTicketAsync(
            reservation.TicketId, 
            reservation.Id);
        
        if (!success)
        {
            _logger.LogWarning("Failed to release ticket {TicketId}: {Message}", 
                reservation.TicketId, message);
            
            return new CancelReservationResponse
            {
                Success = false,
                Message = $"Failed to cancel reservation: {message}"
            };
        }
        
        // Update reservation status
        _reservationRepository.UpdateReservationStatus(request.ReservationId, "Cancelled");
        
        _logger.LogInformation("Reservation with ID {ReservationId} cancelled successfully", request.ReservationId);
        
        return new CancelReservationResponse
        {
            Success = true,
            Message = "Reservation cancelled successfully"
        };
    }

    public override Task<ConfirmPaymentResponse> ConfirmPayment(ConfirmPaymentRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow. ReservationId: {ReservationId}, PaymentMethod: {PaymentMethod}", 
            "ConfirmPayment", request.ReservationId, request.PaymentMethod);
        
        var reservation = _reservationRepository.GetReservationById(request.ReservationId);
        
        if (reservation == null)
        {
            _logger.LogWarning("Reservation with ID {ReservationId} not found", request.ReservationId);
            return Task.FromResult(new ConfirmPaymentResponse
            {
                Success = false,
                Message = $"Reservation with ID {request.ReservationId} not found"
            });
        }
        
        if (reservation.Status == "Cancelled")
        {
            _logger.LogWarning("Cannot confirm payment for cancelled reservation {ReservationId}", request.ReservationId);
            return Task.FromResult(new ConfirmPaymentResponse
            {
                Success = false,
                Message = "Cannot confirm payment for cancelled reservation"
            });
        }
        
        if (reservation.Status == "Paid")
        {
            _logger.LogWarning("Reservation {ReservationId} is already paid", request.ReservationId);
            return Task.FromResult(new ConfirmPaymentResponse
            {
                Success = false,
                Message = "Reservation is already paid"
            });
        }
        
        // Update reservation status
        _reservationRepository.UpdateReservationStatus(request.ReservationId, "Paid");
        
        _logger.LogInformation("Payment confirmed for reservation {ReservationId}", request.ReservationId);
        
        return Task.FromResult(new ConfirmPaymentResponse
        {
            Success = true,
            Message = "Payment confirmed successfully"
        });
    }

    private static ReservationDetails MapToReservationDetails(Reservation reservation)
    {
        return new ReservationDetails
        {
            ReservationId = reservation.Id,
            CustomerId = reservation.CustomerId,
            CustomerName = reservation.CustomerName,
            CustomerEmail = reservation.CustomerEmail,
            TicketId = reservation.TicketId,
            EventName = reservation.EventName,
            EventDate = reservation.EventDate,
            Venue = reservation.Venue,
            Price = reservation.Price,
            Status = reservation.Status,
            CreatedAt = reservation.CreatedAt
        };
    }
} 