# Enterprise Features

This document describes the enterprise-grade features added to SnapcastClient for production use.

## Features Overview

- **Connection Retry Logic**: Automatic reconnection with exponential backoff
- **Health Monitoring**: Periodic connection health checks
- **Comprehensive Logging**: Structured logging with Microsoft.Extensions.Logging
- **Configuration Management**: Options pattern support
- **Dependency Injection**: Full DI container integration

## Quick Start

### Basic Usage with Resilient Connection

```csharp
using Microsoft.Extensions.Logging;
using SnapcastClient;

// Create logger (optional)
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Client>();

// Configure options
var options = new SnapcastClientOptions
{
    EnableAutoReconnect = true,
    MaxRetryAttempts = 5,
    HealthCheckIntervalMs = 30000,
    EnableVerboseLogging = true
};

// Create resilient connection
var connection = new ResilientTcpConnection("127.0.0.1", 1705, options, 
    loggerFactory.CreateLogger<ResilientTcpConnection>());

// Create client with logging
var client = new Client(connection, logger);

// Subscribe to connection events
if (connection is ResilientTcpConnection resilientConn)
{
    resilientConn.OnConnectionStateChanged += state => 
        Console.WriteLine($"Connection state: {state}");
    
    resilientConn.OnReconnectAttempt += (attempt, ex) => 
        Console.WriteLine($"Reconnect attempt {attempt}: {ex?.Message}");
}

// Use the client
var version = await client.ServerGetRpcVersionAsync();
Console.WriteLine($"Server version: {version.Major}.{version.Minor}.{version.Patch}");

// Dispose properly
client.Dispose();
```

### Dependency Injection Integration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SnapcastClient;

var builder = Host.CreateApplicationBuilder(args);

// Add logging
builder.Services.AddLogging(logging => logging.AddConsole());

// Add Snapcast client with resilient connection
builder.Services.AddSnapcastClient("127.0.0.1", 1705, options =>
{
    options.EnableAutoReconnect = true;
    options.MaxRetryAttempts = 5;
    options.HealthCheckIntervalMs = 30000;
    options.EnableVerboseLogging = false; // Set to true for detailed logs
    options.ConnectionTimeoutMs = 10000;
    options.UseExponentialBackoff = true;
});

var host = builder.Build();

// Use the client
var client = host.Services.GetRequiredService<IClient>();
var server = await client.ServerGetStatusAsync();
Console.WriteLine($"Server has {server.Groups.Count} groups");

await host.RunAsync();
```

### Simple Connection (No Resilience)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SnapcastClient;

var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());

// Add simple TCP connection (original behavior)
services.AddSnapcastClientSimple("127.0.0.1", 1705);

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetRequiredService<IClient>();

// Use normally
var status = await client.ServerGetStatusAsync();
```

## Configuration Options

### SnapcastClientOptions

```csharp
var options = new SnapcastClientOptions
{
    // Connection settings
    ConnectionTimeoutMs = 5000,        // Connection timeout (default: 5000ms)
    BufferSize = 1024,                 // Network buffer size (default: 1024 bytes)
    
    // Retry settings
    EnableAutoReconnect = true,        // Enable automatic reconnection (default: true)
    MaxRetryAttempts = 3,              // Max retry attempts (default: 3)
    ReconnectDelayMs = 1000,           // Initial reconnect delay (default: 1000ms)
    MaxReconnectDelayMs = 30000,       // Max reconnect delay (default: 30000ms)
    UseExponentialBackoff = true,      // Use exponential backoff (default: true)
    
    // Health monitoring
    HealthCheckIntervalMs = 30000,     // Health check interval (default: 30000ms, 0 = disabled)
    
    // Logging
    EnableVerboseLogging = false       // Enable detailed logging (default: false)
};
```

## Connection States

The `ResilientTcpConnection` provides real-time connection state monitoring:

```csharp
public enum ConnectionState
{
    Disconnected,    // Not connected
    Connecting,      // Attempting to connect
    Connected,       // Connected and healthy
    Degraded,        // Connected but experiencing issues
    Reconnecting,    // Lost connection, attempting to reconnect
    Failed          // Connection failed, no more retry attempts
}
```

### Monitoring Connection State

```csharp
var connection = new ResilientTcpConnection("127.0.0.1", 1705, options, logger);

connection.OnConnectionStateChanged += state =>
{
    switch (state)
    {
        case ConnectionState.Connected:
            Console.WriteLine("✅ Connected to Snapcast server");
            break;
        case ConnectionState.Reconnecting:
            Console.WriteLine("🔄 Reconnecting to server...");
            break;
        case ConnectionState.Failed:
            Console.WriteLine("❌ Connection failed permanently");
            break;
    }
};

connection.OnReconnectAttempt += (attempt, exception) =>
{
    Console.WriteLine($"Reconnect attempt {attempt}: {exception?.Message ?? "Success"}");
};
```

## Logging Levels

The library uses structured logging with different levels:

- **Trace**: Raw JSON messages sent/received
- **Debug**: Command execution, response handling, connection details
- **Information**: Client lifecycle, connection state changes
- **Warning**: Recoverable errors, missing response handlers
- **Error**: Command failures, connection errors, parsing errors

### Example Logging Configuration

```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
    
    // Enable debug logging for SnapCast components only
    logging.AddFilter("SnapcastClient", LogLevel.Debug);
    
    // Enable trace logging for detailed network debugging
    // logging.AddFilter("SnapcastClient.ResilientTcpConnection", LogLevel.Trace);
});
```

## Error Handling

The enterprise features provide better error handling and recovery:

```csharp
try
{
    var client = serviceProvider.GetRequiredService<IClient>();
    var result = await client.ClientGetStatusAsync("invalid-client-id");
}
catch (CommandException ex)
{
    // Snapcast server returned an error
    Console.WriteLine($"Server error: {ex.Message}");
}
catch (ObjectDisposedException)
{
    // Client was disposed
    Console.WriteLine("Client is no longer available");
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Connection is not available"))
{
    // Connection is down and auto-reconnect failed or is disabled
    Console.WriteLine("Connection unavailable, check server status");
}
```

## Best Practices

1. **Always dispose clients**: Use `using` statements or call `Dispose()` explicitly
2. **Configure appropriate timeouts**: Set realistic connection and retry timeouts
3. **Monitor connection state**: Subscribe to state change events for better UX
4. **Use structured logging**: Configure appropriate log levels for production
5. **Handle exceptions gracefully**: Implement proper error handling for network issues
6. **Configure health checks**: Enable periodic health monitoring for long-running applications

## Migration from Basic Usage

If you're upgrading from the basic `TcpConnection`, here's how to migrate:

### Before (Basic)
```csharp
var connection = new TcpConnection("127.0.0.1", 1705);
var client = new Client(connection);
```

### After (Enterprise)
```csharp
// Option 1: Manual setup with resilient connection
var options = new SnapcastClientOptions { EnableAutoReconnect = true };
var connection = new ResilientTcpConnection("127.0.0.1", 1705, options, logger);
var client = new Client(connection, logger);

// Option 2: Dependency injection
services.AddSnapcastClient("127.0.0.1", 1705, options => 
{
    options.EnableAutoReconnect = true;
});
```

The API remains the same, so your existing code will continue to work unchanged.
