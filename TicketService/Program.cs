using Microsoft.AspNetCore.Server.Kestrel.Core;
using TicketService.Repositories;
using TicketService.Services;

namespace TicketService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel
        builder.WebHost.ConfigureKestrel(options =>
        {
            // Setup a HTTP/2 endpoint without TLS for development
            options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http2);
        });

        // Add services to the container
        builder.Services.AddGrpc();
        builder.Services.AddSingleton<ITicketRepository, InMemoryTicketRepository>();
        
        // Configure logging
        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        app.MapGrpcService<TicketManagerService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}
