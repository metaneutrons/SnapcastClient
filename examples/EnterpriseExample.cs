using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SnapCastNet;

namespace SnapCastNet.Examples;

/// <summary>
/// Example demonstrating enterprise features of SnapCastNet
/// </summary>
public class EnterpriseExample
{
    public static async Task Main(string[] args)
    {
        // Create host builder with dependency injection
        var builder = Host.CreateApplicationBuilder(args);

        // Add logging with console output
        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
            
            // Enable debug logging for SnapCast components
            logging.AddFilter("SnapCastNet", LogLevel.Debug);
        });

        // Add Snapcast client with enterprise features
        builder.Services.AddSnapcastClient("127.0.0.1", 1705, options =>
        {
            options.EnableAutoReconnect = true;
            options.MaxRetryAttempts = 5;
            options.HealthCheckIntervalMs = 30000;
            options.ConnectionTimeoutMs = 10000;
            options.UseExponentialBackoff = true;
            options.EnableVerboseLogging = false; // Set to true for detailed network logs
        });

        var host = builder.Build();
        var logger = host.Services.GetRequiredService<ILogger<EnterpriseExample>>();

        try
        {
            logger.LogInformation("Starting Snapcast enterprise example");

            // Get the client from DI container
            var client = host.Services.GetRequiredService<IClient>();

            // Subscribe to events if using resilient connection
            if (client is Client clientImpl && 
                clientImpl.GetType().GetField("Connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(clientImpl) is ResilientTcpConnection resilientConn)
            {
                resilientConn.OnConnectionStateChanged += state =>
                    logger.LogInformation("Connection state changed to: {State}", state);

                resilientConn.OnReconnectAttempt += (attempt, ex) =>
                    logger.LogWarning("Reconnect attempt {Attempt}: {Error}", attempt, ex?.Message ?? "Success");
            }

            // Test basic functionality
            logger.LogInformation("Getting server RPC version...");
            var version = await client.ServerGetRpcVersionAsync();
            logger.LogInformation("Server RPC version: {Major}.{Minor}.{Patch}", 
                version.Major, version.Minor, version.Patch);

            logger.LogInformation("Getting server status...");
            var server = await client.ServerGetStatusAsync();
            logger.LogInformation("Server has {GroupCount} groups and {StreamCount} streams", 
                server.Groups.Count, server.Streams.Count);

            // List all clients
            foreach (var group in server.Groups)
            {
                logger.LogInformation("Group '{GroupName}' ({GroupId}) has {ClientCount} clients", 
                    group.Name, group.Id, group.Clients.Count);
                
                foreach (var clientInfo in group.Clients)
                {
                    logger.LogInformation("  - Client '{ClientName}' ({ClientId}) - Volume: {Volume}%", 
                        clientInfo.Config.Name, clientInfo.Id, clientInfo.Config.Volume.Percent);
                }
            }

            // Subscribe to real-time events
            client.OnClientConnect = (connectedClient) =>
                logger.LogInformation("Client connected: {ClientName} ({ClientId})", 
                    connectedClient.Config.Name, connectedClient.Id);

            client.OnClientDisconnect = (disconnectedClient) =>
                logger.LogInformation("Client disconnected: {ClientName} ({ClientId})", 
                    disconnectedClient.Config.Name, disconnectedClient.Id);

            client.OnServerUpdate = (updatedServer) =>
                logger.LogInformation("Server updated - Groups: {GroupCount}, Streams: {StreamCount}", 
                    updatedServer.Groups.Count, updatedServer.Streams.Count);

            logger.LogInformation("Listening for events... Press Ctrl+C to exit");

            // Keep the application running
            var cancellationToken = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cancellationToken.Cancel();
            };

            try
            {
                await Task.Delay(-1, cancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Shutting down...");
            }

            // Dispose the client properly
            if (client is IDisposable disposableClient)
            {
                disposableClient.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred: {Message}", ex.Message);
        }
        finally
        {
            await host.StopAsync();
        }
    }
}
