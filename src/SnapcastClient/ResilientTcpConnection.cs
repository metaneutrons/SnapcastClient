/***
    This file is part of snapcast-net
    Copyright (C) 2024  Craig Sturdy

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
***/

using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace SnapcastClient;

/// <summary>
/// A resilient TCP connection wrapper that provides retry logic, health monitoring, and automatic reconnection
/// </summary>
public class ResilientTcpConnection : IConnection, IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly ILogger<ResilientTcpConnection>? _logger;
    private readonly SnapcastClientOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly object _connectionLock = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1); // Prevent concurrent connection attempts

    private TcpConnection? _connection;
    private ConnectionState _connectionState = ConnectionState.Disconnected;
    private Timer? _healthCheckTimer;
    private int _reconnectAttempts = 0;
    private DateTime _lastHealthCheck;
    private bool _disposed = false;
    private bool _isConnecting = false; // Track if connection attempt is in progress

    /// <summary>
    /// Initializes a new instance of the ResilientTcpConnection class.
    /// </summary>
    /// <param name="host">The hostname or IP address of the server.</param>
    /// <param name="port">The port number to connect to.</param>
    /// <param name="options">Optional configuration options for the connection.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <param name="timeProvider">Optional time provider for testing purposes.</param>
    public ResilientTcpConnection(
        string host,
        int port,
        SnapcastClientOptions? options = null,
        ILogger<ResilientTcpConnection>? logger = null,
        TimeProvider? timeProvider = null)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _port = port;
        _options = options ?? new SnapcastClientOptions();
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _lastHealthCheck = _timeProvider.GetUtcNow().DateTime;

        InitializeConnection();
    }

    /// <summary>
    /// Event fired when the connection state changes
    /// </summary>
    public event Action<ConnectionState>? OnConnectionStateChanged;

    /// <summary>
    /// Event fired when a reconnection attempt is made
    /// </summary>
    public event Action<int, Exception?>? OnReconnectAttempt;

    /// <summary>
    /// Current connection state
    /// </summary>
    public ConnectionState State => _connectionState;

    /// <summary>
    /// Number of reconnection attempts made
    /// </summary>
    public int ReconnectAttempts => _reconnectAttempts;

    /// <summary>
    /// Last time a health check was performed
    /// </summary>
    public DateTime LastHealthCheck => _lastHealthCheck;

    // Constructor body for initialization
    static ResilientTcpConnection()
    {
        // Static constructor if needed
    }

    // Instance initialization
    private void InitializeConnection()
    {
        _logger?.LogInformation("Creating resilient TCP connection to {Host}:{Port}", _host, _port);

        // Start health check timer if enabled
        if (_options.HealthCheckIntervalMs > 0)
        {
            _healthCheckTimer = new Timer(
                PerformHealthCheck,
                null,
                TimeSpan.FromMilliseconds(_options.HealthCheckIntervalMs),
                TimeSpan.FromMilliseconds(_options.HealthCheckIntervalMs)
            );
        }

        // Initial connection attempt
        _ = Task.Run(async () => await ConnectAsync(isInitialConnection: true));
    }

    /// <summary>
    /// Sends data to the server with automatic reconnection if the connection is lost.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the connection has been disposed.</exception>
    public void Send(string data)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ResilientTcpConnection));

        lock (_connectionLock)
        {
            try
            {
                if (!EnsureConnected())
                {
                    // Connection not available, throw exception for Send since it's a critical operation
                    throw new InvalidOperationException("Connection is not available and reconnection is in progress");
                }

                _connection?.Send(data);

                if (_options.EnableVerboseLogging)
                {
                    _logger?.LogDebug("Sent data: {Data}", data);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send data: {Data}", data);
                HandleConnectionError(ex);
                throw;
            }
        }
    }

    /// <summary>
    /// Reads data from the server with automatic reconnection if the connection is lost.
    /// </summary>
    /// <returns>The data received from the server, or null if no data is available or connection is disposed.</returns>
    public string? Read()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ResilientTcpConnection));

        lock (_connectionLock)
        {
            try
            {
                if (!EnsureConnected())
                {
                    // Connection not available, but reconnection may be in progress
                    // Log at debug level instead of error to reduce noise
                    _logger?.LogDebug("Connection not available, reconnection may be in progress");
                    return null;
                }

                var result = _connection?.Read();

                if (_options.EnableVerboseLogging && result != null)
                {
                    _logger?.LogDebug("Received data: {Data}", result);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to read data");
                HandleConnectionError(ex);
                return null;
            }
        }
    }

    private bool EnsureConnected()
    {
        if (_connection == null || _connectionState != ConnectionState.Connected)
        {
            if (_options.EnableAutoReconnect)
            {
                // Start reconnection if not already in progress
                if (!_isConnecting && _connectionState != ConnectionState.Connecting && _connectionState != ConnectionState.Reconnecting)
                {
                    _ = Task.Run(async () => await ConnectAsync(isInitialConnection: false));
                }
                // Return false to indicate connection is not available, but don't throw
                return false;
            }
            else
            {
                throw new InvalidOperationException("Connection is not available and auto-reconnect is disabled");
            }
        }
        return true;
    }

    private async Task ConnectAsync(bool isInitialConnection = false)
    {
        if (_disposed)
            return;

        // Prevent concurrent connection attempts
        if (!await _connectionSemaphore.WaitAsync(0)) // Don't wait, just check if available
        {
            _logger?.LogDebug("Connection attempt already in progress, skipping");
            return;
        }

        try
        {
            _isConnecting = true;
            
            // Reset attempt counter for initial connections
            if (isInitialConnection)
            {
                _reconnectAttempts = 0;
            }

            var delay = _options.ReconnectDelayMs;
            var maxAttempts = _options.MaxRetryAttempts;

            while (_reconnectAttempts < maxAttempts && !_disposed)
            {
                try
                {
                    SetConnectionState(ConnectionState.Connecting);
                    _logger?.LogInformation(
                        "Attempting to connect to {Host}:{Port} (attempt {Attempt}/{MaxAttempts})",
                        _host,
                        _port,
                        _reconnectAttempts + 1,
                        maxAttempts
                    );

                    lock (_connectionLock)
                    {
                        _connection?.Dispose();
                        _connection = new TcpConnection(_host, _port);
                    }

                    SetConnectionState(ConnectionState.Connected);
                    _reconnectAttempts = 0;
                    _logger?.LogInformation("Successfully connected to {Host}:{Port}", _host, _port);
                    return;
                }
                catch (Exception ex)
                {
                    _reconnectAttempts++;
                    _logger?.LogWarning(ex, "Connection attempt {Attempt} failed: {Message}", _reconnectAttempts, ex.Message);

                    OnReconnectAttempt?.Invoke(_reconnectAttempts, ex);

                    if (_reconnectAttempts >= maxAttempts)
                    {
                        SetConnectionState(ConnectionState.Failed);
                        _logger?.LogError("All connection attempts failed. Giving up.");
                        
                        // Wait before allowing another connection cycle to prevent tight loops
                        await Task.Delay(TimeSpan.FromSeconds(30), _cancellationTokenSource.Token);
                        _reconnectAttempts = 0; // Reset for next cycle
                        return;
                    }

                    SetConnectionState(ConnectionState.Reconnecting);

                    // Exponential backoff
                    if (_options.UseExponentialBackoff)
                    {
                        delay = Math.Min(delay * 2, _options.MaxReconnectDelayMs);
                    }

                    try
                    {
                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
        }
        finally
        {
            _isConnecting = false;
            _connectionSemaphore.Release();
        }
    }

    private void HandleConnectionError(Exception ex)
    {
        _logger?.LogWarning(ex, "Connection error detected: {Message}", ex.Message);

        if (_connectionState == ConnectionState.Connected)
        {
            SetConnectionState(ConnectionState.Degraded);
        }

        // Only start reconnection if auto-reconnect is enabled and no connection attempt is in progress
        if (_options.EnableAutoReconnect && !_isConnecting && _connectionState != ConnectionState.Connecting)
        {
            _ = Task.Run(async () => await ConnectAsync(isInitialConnection: false));
        }
    }

    private void PerformHealthCheck(object? state)
    {
        if (_disposed)
            return;

        try
        {
            _lastHealthCheck = _timeProvider.GetUtcNow().DateTime;

            lock (_connectionLock)
            {
                if (_connection == null)
                {
                    if (_connectionState == ConnectionState.Connected)
                    {
                        SetConnectionState(ConnectionState.Disconnected);
                    }
                    return;
                }

                // Simple health check - try to access the underlying connection
                // In a real implementation, you might send a ping command
                if (_connectionState == ConnectionState.Degraded)
                {
                    _logger?.LogInformation("Health check: Connection appears to be recovering");
                    SetConnectionState(ConnectionState.Connected);
                }
            }

            _logger?.LogDebug("Health check completed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Health check failed: {Message}", ex.Message);
            HandleConnectionError(ex);
        }
    }

    private void SetConnectionState(ConnectionState newState)
    {
        if (_connectionState != newState)
        {
            var oldState = _connectionState;
            _connectionState = newState;

            _logger?.LogInformation("Connection state changed from {OldState} to {NewState}", oldState, newState);
            OnConnectionStateChanged?.Invoke(newState);
        }
    }

    /// <summary>
    /// Releases all resources used by the ResilientTcpConnection.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _cancellationTokenSource.Cancel();

        _healthCheckTimer?.Dispose();

        lock (_connectionLock)
        {
            _connection?.Dispose();
        }

        _cancellationTokenSource.Dispose();
        _connectionSemaphore.Dispose();
        SetConnectionState(ConnectionState.Disconnected);

        _logger?.LogInformation("ResilientTcpConnection disposed");
    }
}
