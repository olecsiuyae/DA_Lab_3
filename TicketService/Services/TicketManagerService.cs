using Grpc.Core;
using Microsoft.Extensions.Logging;
using TicketService.Protos;
using TicketService.Repositories;

namespace TicketService.Services;

public class TicketManagerService : TicketManager.TicketManagerBase
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<TicketManagerService> _logger;

    public TicketManagerService(ITicketRepository ticketRepository, ILogger<TicketManagerService> logger)
    {
        _ticketRepository = ticketRepository;
        _logger = logger;
        _logger.LogInformation("{Method} flow.", "TicketManagerService constructor");
    }

    public override Task<GetAllTicketsResponse> GetAllTickets(GetAllTicketsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow.", "GetAllTickets");
        
        var tickets = _ticketRepository.GetAllTickets();
        var response = new GetAllTicketsResponse();
        
        foreach (var ticket in tickets)
        {
            response.Tickets.Add(new TicketInfo
            {
                TicketId = ticket.Id,
                EventName = ticket.EventName,
                EventDate = ticket.EventDate,
                Venue = ticket.Venue,
                Price = ticket.Price,
                IsAvailable = ticket.IsAvailable
            });
        }
        
        _logger.LogInformation("GetAllTickets returning {Count} tickets", response.Tickets.Count);
        
        return Task.FromResult(response);
    }

    public override Task<TicketDetails> GetTicketById(GetTicketByIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}", "GetTicketById", request.TicketId);
        
        var ticket = _ticketRepository.GetTicketById(request.TicketId);
        
        if (ticket == null)
        {
            _logger.LogWarning("Ticket with ID {TicketId} not found", request.TicketId);
            throw new RpcException(new Status(StatusCode.NotFound, $"Ticket with ID {request.TicketId} not found"));
        }
        
        var response = new TicketDetails
        {
            TicketId = ticket.Id,
            EventName = ticket.EventName,
            EventDate = ticket.EventDate,
            Venue = ticket.Venue,
            Price = ticket.Price,
            IsAvailable = ticket.IsAvailable,
            Section = ticket.Section,
            Row = ticket.Row,
            Seat = ticket.Seat
        };
        
        _logger.LogInformation("GetTicketById successful for ticket ID {TicketId}", request.TicketId);
        
        return Task.FromResult(response);
    }

    public override Task<CheckAvailabilityResponse> CheckAvailability(CheckAvailabilityRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}", "CheckAvailability", request.TicketId);
        
        var isAvailable = _ticketRepository.IsTicketAvailable(request.TicketId);
        var response = new CheckAvailabilityResponse { IsAvailable = isAvailable };
        
        _logger.LogInformation("CheckAvailability for ticket ID {TicketId}: {IsAvailable}", 
            request.TicketId, isAvailable);
        
        return Task.FromResult(response);
    }

    public override Task<ReserveTicketResponse> ReserveTicket(ReserveTicketRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}, ReservationId: {ReservationId}, CustomerId: {CustomerId}", 
            "ReserveTicket", request.TicketId, request.ReservationId, request.CustomerId);
        
        var success = _ticketRepository.ReserveTicket(
            request.TicketId, 
            request.ReservationId, 
            request.CustomerId);
        
        var response = new ReserveTicketResponse
        {
            Success = success,
            Message = success 
                ? $"Ticket {request.TicketId} successfully reserved" 
                : $"Failed to reserve ticket {request.TicketId}"
        };
        
        _logger.LogInformation("ReserveTicket for ticket ID {TicketId}: {Success}", 
            request.TicketId, success);
        
        return Task.FromResult(response);
    }

    public override Task<ReleaseTicketResponse> ReleaseTicket(ReleaseTicketRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{Method} flow. TicketId: {TicketId}, ReservationId: {ReservationId}", 
            "ReleaseTicket", request.TicketId, request.ReservationId);
        
        var success = _ticketRepository.ReleaseTicket(request.TicketId, request.ReservationId);
        
        var response = new ReleaseTicketResponse
        {
            Success = success,
            Message = success 
                ? $"Ticket {request.TicketId} successfully released" 
                : $"Failed to release ticket {request.TicketId}"
        };
        
        _logger.LogInformation("ReleaseTicket for ticket ID {TicketId}: {Success}", 
            request.TicketId, success);
        
        return Task.FromResult(response);
    }
} 