using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using TicketService.Protos;

namespace ReservationService.Services;

/// <summary>
/// Client for interacting with the TicketService
/// </summary>
public interface ITicketServiceClient
{
    /// <summary>
    /// Get all available tickets
    /// </summary>
    /// <returns>List of ticket information</returns>
    Task<List<TicketInfo>> GetAllTicketsAsync();
    
    /// <summary>
    /// Get ticket details by ID
    /// </summary>
    /// <param name="ticketId">ID of the ticket to retrieve</param>
    /// <returns>Ticket details if found</returns>
    Task<TicketDetails> GetTicketByIdAsync(string ticketId);
    
    /// <summary>
    /// Check if a ticket is available
    /// </summary>
    /// <param name="ticketId">ID of the ticket to check</param>
    /// <returns>True if available, false otherwise</returns>
    Task<bool> CheckAvailabilityAsync(string ticketId);
    
    /// <summary>
    /// Reserve a ticket
    /// </summary>
    /// <param name="ticketId">ID of the ticket to reserve</param>
    /// <param name="reservationId">ID of the reservation</param>
    /// <param name="customerId">ID of the customer</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<(bool Success, string Message)> ReserveTicketAsync(string ticketId, string reservationId, string customerId);
    
    /// <summary>
    /// Release a previously reserved ticket
    /// </summary>
    /// <param name="ticketId">ID of the ticket to release</param>
    /// <param name="reservationId">ID of the reservation</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<(bool Success, string Message)> ReleaseTicketAsync(string ticketId, string reservationId);
}

/// <inheritdoc/>
public class TicketServiceClient : ITicketServiceClient
{
    private readonly TicketManager.TicketManagerClient _client;
    private readonly ILogger<TicketServiceClient> _logger;

    public TicketServiceClient(ILogger<TicketServiceClient> logger)
    {
        _logger = logger;
        _logger.LogInformation("{Method} flow.", "TicketServiceClient constructor");
        
        var channel = GrpcChannel.ForAddress("http://localhost:5001");
        _client = new TicketManager.TicketManagerClient(channel);
    }

    public async Task<List<TicketInfo>> GetAllTicketsAsync()
    {
        _logger.LogInformation("{Method} flow.", "GetAllTicketsAsync");
        
        try
        {
            var response = await _client.GetAllTicketsAsync(new GetAllTicketsRequest());
            return response.Tickets.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tickets from TicketService");
            return new List<TicketInfo>();
        }
    }

    public async Task<TicketDetails> GetTicketByIdAsync(string ticketId)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}", "GetTicketByIdAsync", ticketId);
        
        try
        {
            var request = new GetTicketByIdRequest { TicketId = ticketId };
            return await _client.GetTicketByIdAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket {TicketId} from TicketService", ticketId);
            throw;
        }
    }

    public async Task<bool> CheckAvailabilityAsync(string ticketId)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}", "CheckAvailabilityAsync", ticketId);
        
        try
        {
            var request = new CheckAvailabilityRequest { TicketId = ticketId };
            var response = await _client.CheckAvailabilityAsync(request);
            return response.IsAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for ticket {TicketId}", ticketId);
            return false;
        }
    }

    public async Task<(bool Success, string Message)> ReserveTicketAsync(string ticketId, string reservationId, string customerId)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}, ReservationId: {ReservationId}, CustomerId: {CustomerId}", 
            "ReserveTicketAsync", ticketId, reservationId, customerId);
        
        try
        {
            var request = new ReserveTicketRequest
            {
                TicketId = ticketId,
                ReservationId = reservationId,
                CustomerId = customerId
            };
            
            var response = await _client.ReserveTicketAsync(request);
            return (response.Success, response.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving ticket {TicketId}", ticketId);
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ReleaseTicketAsync(string ticketId, string reservationId)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}, ReservationId: {ReservationId}", 
            "ReleaseTicketAsync", ticketId, reservationId);
        
        try
        {
            var request = new ReleaseTicketRequest
            {
                TicketId = ticketId,
                ReservationId = reservationId
            };
            
            var response = await _client.ReleaseTicketAsync(request);
            return (response.Success, response.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing ticket {TicketId}", ticketId);
            return (false, $"Error: {ex.Message}");
        }
    }
} 