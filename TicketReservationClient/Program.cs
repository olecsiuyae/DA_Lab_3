using Grpc.Net.Client;
using Newtonsoft.Json;
using ReservationService.Protos;
using TicketService.Protos;

namespace TicketReservationClient;

public class Program
{
    private static readonly string TicketServiceUrl = "http://localhost:5001";
    private static readonly string ReservationServiceUrl = "http://localhost:5002";
    
    private static TicketManager.TicketManagerClient? _ticketClient;
    private static ReservationManager.ReservationManagerClient? _reservationClient;
    
    private static string _customerId = "C1001";
    private static string _customerName = "John Doe";
    private static string _customerEmail = "john.doe@example.com";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Ticket Reservation Client");
        Console.WriteLine("=========================");
        
        try
        {
            InitializeClients();
            
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nPlease select an option:");
                Console.WriteLine("1. View all available tickets");
                Console.WriteLine("2. View ticket details");
                Console.WriteLine("3. Make a reservation");
                Console.WriteLine("4. View my reservations");
                Console.WriteLine("5. Cancel a reservation");
                Console.WriteLine("6. Confirm payment for a reservation");
                Console.WriteLine("0. Exit");
                
                Console.Write("\nYour choice: ");
                var choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        await ViewAllTickets();
                        break;
                    case "2":
                        await ViewTicketDetails();
                        break;
                    case "3":
                        await MakeReservation();
                        break;
                    case "4":
                        await ViewMyReservations();
                        break;
                    case "5":
                        await CancelReservation();
                        break;
                    case "6":
                        await ConfirmPayment();
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        
        Console.WriteLine("Thank you for using the Ticket Reservation System!");
    }

    private static void InitializeClients()
    {
        var ticketChannel = GrpcChannel.ForAddress(TicketServiceUrl);
        _ticketClient = new TicketManager.TicketManagerClient(ticketChannel);
        
        var reservationChannel = GrpcChannel.ForAddress(ReservationServiceUrl);
        _reservationClient = new ReservationManager.ReservationManagerClient(reservationChannel);
        
        Console.WriteLine("Connected to services successfully.");
    }

    private static async Task ViewAllTickets()
    {
        Console.WriteLine("\nFetching all available tickets...");
        
        try
        {
            var response = await _ticketClient!.GetAllTicketsAsync(new GetAllTicketsRequest());
            
            if (response.Tickets.Count == 0)
            {
                Console.WriteLine("No tickets available.");
                return;
            }
            
            Console.WriteLine("\nAvailable Tickets:");
            Console.WriteLine("=================");
            
            foreach (var ticket in response.Tickets)
            {
                Console.WriteLine($"ID: {ticket.TicketId}");
                Console.WriteLine($"Event: {ticket.EventName}");
                Console.WriteLine($"Date: {ticket.EventDate}");
                Console.WriteLine($"Venue: {ticket.Venue}");
                Console.WriteLine($"Price: ${ticket.Price:F2}");
                Console.WriteLine($"Available: {(ticket.IsAvailable ? "Yes" : "No")}");
                Console.WriteLine("-------------------");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching tickets: {ex.Message}");
        }
    }

    private static async Task ViewTicketDetails()
    {
        Console.Write("\nEnter ticket ID: ");
        var ticketId = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(ticketId))
        {
            Console.WriteLine("Invalid ticket ID.");
            return;
        }
        
        try
        {
            var request = new GetTicketByIdRequest { TicketId = ticketId };
            var ticket = await _ticketClient!.GetTicketByIdAsync(request);
            
            Console.WriteLine("\nTicket Details:");
            Console.WriteLine("==============");
            Console.WriteLine($"ID: {ticket.TicketId}");
            Console.WriteLine($"Event: {ticket.EventName}");
            Console.WriteLine($"Date: {ticket.EventDate}");
            Console.WriteLine($"Venue: {ticket.Venue}");
            Console.WriteLine($"Section: {ticket.Section}");
            Console.WriteLine($"Row: {ticket.Row}");
            Console.WriteLine($"Seat: {ticket.Seat}");
            Console.WriteLine($"Price: ${ticket.Price:F2}");
            Console.WriteLine($"Available: {(ticket.IsAvailable ? "Yes" : "No")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching ticket details: {ex.Message}");
        }
    }

    private static async Task MakeReservation()
    {
        Console.Write("\nEnter ticket ID to reserve: ");
        var ticketId = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(ticketId))
        {
            Console.WriteLine("Invalid ticket ID.");
            return;
        }
        
        try
        {
            // First check if the ticket is available
            var availabilityRequest = new CheckAvailabilityRequest { TicketId = ticketId };
            var availabilityResponse = await _ticketClient!.CheckAvailabilityAsync(availabilityRequest);
            
            if (!availabilityResponse.IsAvailable)
            {
                Console.WriteLine("This ticket is not available for reservation.");
                return;
            }
            
            // Create the reservation
            var reservationRequest = new CreateReservationRequest
            {
                CustomerId = _customerId,
                CustomerName = _customerName,
                CustomerEmail = _customerEmail,
                TicketId = ticketId
            };
            
            var reservationResponse = await _reservationClient!.CreateReservationAsync(reservationRequest);
            
            if (reservationResponse.Success)
            {
                Console.WriteLine("\nReservation created successfully!");
                Console.WriteLine($"Reservation ID: {reservationResponse.ReservationId}");
                Console.WriteLine("\nReservation Details:");
                Console.WriteLine("===================");
                Console.WriteLine($"Event: {reservationResponse.Reservation.EventName}");
                Console.WriteLine($"Date: {reservationResponse.Reservation.EventDate}");
                Console.WriteLine($"Venue: {reservationResponse.Reservation.Venue}");
                Console.WriteLine($"Price: ${reservationResponse.Reservation.Price:F2}");
                Console.WriteLine($"Status: {reservationResponse.Reservation.Status}");
                Console.WriteLine($"Created: {reservationResponse.Reservation.CreatedAt}");
            }
            else
            {
                Console.WriteLine($"Failed to create reservation: {reservationResponse.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error making reservation: {ex.Message}");
        }
    }

    private static async Task ViewMyReservations()
    {
        try
        {
            var request = new GetCustomerReservationsRequest { CustomerId = _customerId };
            var response = await _reservationClient!.GetCustomerReservationsAsync(request);
            
            if (response.Reservations.Count == 0)
            {
                Console.WriteLine("\nYou have no reservations.");
                return;
            }
            
            Console.WriteLine("\nYour Reservations:");
            Console.WriteLine("=================");
            
            foreach (var reservation in response.Reservations)
            {
                Console.WriteLine($"Reservation ID: {reservation.ReservationId}");
                Console.WriteLine($"Event: {reservation.EventName}");
                Console.WriteLine($"Date: {reservation.EventDate}");
                Console.WriteLine($"Venue: {reservation.Venue}");
                Console.WriteLine($"Ticket ID: {reservation.TicketId}");
                Console.WriteLine($"Price: ${reservation.Price:F2}");
                Console.WriteLine($"Status: {reservation.Status}");
                Console.WriteLine($"Created: {reservation.CreatedAt}");
                Console.WriteLine("-------------------");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching reservations: {ex.Message}");
        }
    }

    private static async Task CancelReservation()
    {
        Console.Write("\nEnter reservation ID to cancel: ");
        var reservationId = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(reservationId))
        {
            Console.WriteLine("Invalid reservation ID.");
            return;
        }
        
        try
        {
            var request = new CancelReservationRequest { ReservationId = reservationId };
            var response = await _reservationClient!.CancelReservationAsync(request);
            
            if (response.Success)
            {
                Console.WriteLine("Reservation cancelled successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to cancel reservation: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cancelling reservation: {ex.Message}");
        }
    }

    private static async Task ConfirmPayment()
    {
        Console.Write("\nEnter reservation ID to confirm payment: ");
        var reservationId = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(reservationId))
        {
            Console.WriteLine("Invalid reservation ID.");
            return;
        }
        
        Console.Write("Enter payment method (Credit Card, PayPal, etc.): ");
        var paymentMethod = Console.ReadLine() ?? "Credit Card";
        
        var paymentId = Guid.NewGuid().ToString("N");
        
        try
        {
            var request = new ConfirmPaymentRequest
            {
                ReservationId = reservationId,
                PaymentMethod = paymentMethod,
                PaymentId = paymentId
            };
            
            var response = await _reservationClient!.ConfirmPaymentAsync(request);
            
            if (response.Success)
            {
                Console.WriteLine("Payment confirmed successfully.");
                Console.WriteLine($"Payment ID: {paymentId}");
            }
            else
            {
                Console.WriteLine($"Failed to confirm payment: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error confirming payment: {ex.Message}");
        }
    }
}
