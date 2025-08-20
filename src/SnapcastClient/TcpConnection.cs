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
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace SnapcastClient;

/// <summary>
/// TCP connection implementation for communicating with the Snapcast server.
/// </summary>
public class TcpConnection : IConnection, IDisposable
{
    private readonly TcpClient Client;
    private readonly NetworkStream Stream;
    private readonly ILogger<TcpConnection>? _logger;
    private readonly StringBuilder _messageBuffer = new();
    private bool _disposed = false;

    // Enterprise-grade configuration
    private readonly TimeSpan _readTimeout = TimeSpan.FromSeconds(5);
    private readonly int _bufferSize = 4096;

    // Producer-Consumer Pattern (Phase 2)
    private readonly Channel<string> _messageChannel;
    private readonly ChannelWriter<string> _messageWriter;
    private readonly ChannelReader<string> _messageReader;
    private readonly CancellationTokenSource _processingCts = new();
    private Task? _messageProcessingTask;
    private DateTime _lastMessageReceived = DateTime.UtcNow;

    // Circuit Breaker Pattern (Phase 3)
    private readonly CircuitBreaker _circuitBreaker;
    private readonly ConnectionHealthMonitor _healthMonitor;

    // Events for message processing
    public event Action<string>? MessageReceived;
    public event Action<Exception>? ProcessingError;
    public event Action<bool>? ConnectionHealthChanged;
    public event Action<CircuitBreakerState, CircuitBreakerState>? CircuitBreakerStateChanged;

    /// <summary>
    /// Initializes a new instance of the TcpConnection class.
    /// </summary>
    /// <param name="host">The hostname or IP address of the server.</param>
    /// <param name="port">The port number to connect to.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="circuitBreakerOptions">Optional circuit breaker configuration.</param>
    public TcpConnection(string host, int port, ILogger<TcpConnection>? logger = null, CircuitBreakerOptions? circuitBreakerOptions = null)
    {
        _logger = logger;
        Client = new TcpClient(host, port);
        Stream = Client.GetStream();
        
        // Initialize producer-consumer channel with bounded capacity for backpressure
        var channelOptions = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = true
        };
        
        _messageChannel = Channel.CreateBounded<string>(channelOptions);
        _messageWriter = _messageChannel.Writer;
        _messageReader = _messageChannel.Reader;

        // Initialize circuit breaker (Phase 3)
        _circuitBreaker = new CircuitBreaker(circuitBreakerOptions, logger);
        _circuitBreaker.StateChanged += OnCircuitBreakerStateChanged;

        // Initialize health monitor (Phase 3)
        _healthMonitor = new ConnectionHealthMonitor(logger);
        _healthMonitor.ConnectionHealthChanged += OnHealthMonitorStatusChanged;
        
        _logger?.LogDebug("TCP connection established to {Host}:{Port} with producer-consumer pattern and circuit breaker", host, port);
    }

    /// <summary>
    /// Sends data to the server over the TCP connection.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the connection has been disposed.</exception>
    public void Send(string data)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TcpConnection));

        byte[] bytes = Encoding.UTF8.GetBytes(data + '\n');
        Stream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Reads data from the server over the TCP connection (legacy synchronous method).
    /// </summary>
    /// <returns>The data received from the server, or null if no data is available or connection is disposed.</returns>
    [Obsolete("Use ReadAsync for better performance and timeout handling")]
    public string? Read()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TcpConnection));

        if (!Stream.DataAvailable)
            return null;

        string responseData = "";
        int chunkSize = 1024;
        byte[] responseBytes = new byte[chunkSize];
        int braceCount = 0;
        bool inString = false;
        bool escapeNext = false;
        bool jsonStarted = false;

        int bytesRead;
        while ((bytesRead = Stream.Read(responseBytes, 0, responseBytes.Length)) > 0)
        {
            string chunk = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);
            responseData += chunk;

            // Parse character by character to find complete JSON object
            for (int i = responseData.Length - chunk.Length; i < responseData.Length; i++)
            {
                char c = responseData[i];

                if (escapeNext)
                {
                    escapeNext = false;
                    continue;
                }

                if (c == '\\' && inString)
                {
                    escapeNext = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (!inString)
                {
                    if (c == '{')
                    {
                        braceCount++;
                        jsonStarted = true;
                    }
                    else if (c == '}')
                    {
                        braceCount--;
                        
                        // Complete JSON object found
                        if (jsonStarted && braceCount == 0)
                        {
                            // Look for the terminating newline
                            if (i + 1 < responseData.Length && responseData[i + 1] == '\n')
                            {
                                return responseData.Substring(0, i + 1);
                            }
                            // If we're at the end, check if next read gives us the newline
                            if (i + 1 == responseData.Length)
                            {
                                // Continue reading to get the newline
                                break;
                            }
                        }
                    }
                }
            }

            // If we have a complete JSON object followed by newline, return it
            if (jsonStarted && braceCount == 0 && responseData.EndsWith('\n'))
            {
                return responseData.Substring(0, responseData.Length - 1);
            }
        }

        // If we exit the loop, return what we have (fallback for malformed JSON)
        if (responseData.EndsWith('\n'))
            responseData = responseData.Substring(0, responseData.Length - 1);

        return responseData.Length > 0 ? responseData : null;
    }

    /// <summary>
    /// Enterprise-grade async method to read data from the server with timeout, cancellation support, and circuit breaker protection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The complete JSON message received from the server, or null if no data is available.</returns>
    public async Task<string?> ReadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                using var timeoutCts = new CancellationTokenSource(_readTimeout);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, timeoutCts.Token);

                try
                {
                    if (_disposed)
                        throw new ObjectDisposedException(nameof(TcpConnection));

                    // Check if data is available without blocking
                    if (!Stream.DataAvailable)
                    {
                        // Wait a short time for data to become available
                        await Task.Delay(10, combinedCts.Token);
                        if (!Stream.DataAvailable)
                            return null;
                    }

                    var buffer = new byte[_bufferSize];
                    var bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length, combinedCts.Token);
                    
                    if (bytesRead == 0)
                    {
                        _logger?.LogDebug("No bytes read from TCP stream");
                        return null;
                    }

                    var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _logger?.LogTrace("Read {BytesRead} bytes from TCP stream: {Chunk}", bytesRead, chunk);
                    
                    // Record successful message reception for health monitoring
                    _healthMonitor.RecordMessageReceived();
                    
                    return await ProcessMessageChunkAsync(chunk, combinedCts.Token);
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    _logger?.LogWarning("TCP read operation timed out after {Timeout}ms", _readTimeout.TotalMilliseconds);
                    return null;
                }
            });
        }
        catch (CircuitBreakerOpenException ex)
        {
            _logger?.LogWarning("Circuit breaker is open - read operation blocked: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during async TCP read operation");
            throw;
        }
    }

    /// <summary>
    /// Process a chunk of data and extract complete JSON messages using streaming parser.
    /// </summary>
    /// <param name="chunk">The data chunk to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete JSON message or null if incomplete.</returns>
    private async Task<string?> ProcessMessageChunkAsync(string chunk, CancellationToken cancellationToken)
    {
        _messageBuffer.Append(chunk);
        var bufferContent = _messageBuffer.ToString();

        try
        {
            // Use System.Text.Json for better performance
            using var document = JsonDocument.Parse(bufferContent);
            
            // If we successfully parsed the JSON, we have a complete message
            var completeMessage = bufferContent.TrimEnd('\n');
            _messageBuffer.Clear();
            
            _logger?.LogTrace("Successfully parsed complete JSON message: {Message}", completeMessage);
            return completeMessage;
        }
        catch (JsonException)
        {
            // JSON is incomplete, check if we have multiple lines
            var lines = bufferContent.Split('\n');
            
            for (int i = 0; i < lines.Length - 1; i++) // Process all complete lines
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    using var document = JsonDocument.Parse(line);
                    
                    // Found a complete JSON message
                    _logger?.LogTrace("Found complete JSON message in buffer: {Message}", line);
                    
                    // Remove processed lines from buffer
                    var remainingLines = lines.Skip(i + 1);
                    _messageBuffer.Clear();
                    _messageBuffer.Append(string.Join('\n', remainingLines));
                    
                    return line;
                }
                catch (JsonException)
                {
                    // This line is not complete JSON, continue
                    continue;
                }
            }

            // No complete JSON found, keep buffering
            _logger?.LogTrace("JSON incomplete, buffering {Length} characters", bufferContent.Length);
            return null;
        }
    }

    /// <summary>
    /// Starts the producer-consumer message processing pipeline (Phase 2 enhancement).
    /// This enables concurrent message processing with automatic error recovery.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop processing.</param>
    /// <returns>Task representing the message processing operation.</returns>
    public async Task StartMessageProcessingAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TcpConnection));

        if (_messageProcessingTask != null)
        {
            _logger?.LogWarning("Message processing is already running");
            return;
        }

        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _processingCts.Token);

        _logger?.LogInformation("Starting producer-consumer message processing pipeline");

        // Producer: Read messages from TCP stream
        var producerTask = Task.Run(async () =>
        {
            try
            {
                while (!combinedCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var message = await ReadAsync(combinedCts.Token);
                        if (message != null)
                        {
                            _lastMessageReceived = DateTime.UtcNow;
                            await _messageWriter.WriteAsync(message, combinedCts.Token);
                            _logger?.LogTrace("Producer queued message: {Message}", message);
                        }
                        else
                        {
                            // No message available, brief delay to prevent tight loop
                            await Task.Delay(10, combinedCts.Token);
                        }
                    }
                    catch (OperationCanceledException) when (combinedCts.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error in message producer");
                        ProcessingError?.Invoke(ex);
                        
                        // Brief delay before retry to prevent tight error loop
                        await Task.Delay(1000, combinedCts.Token);
                    }
                }
            }
            finally
            {
                _messageWriter.Complete();
                _logger?.LogDebug("Message producer completed");
            }
        }, combinedCts.Token);

        // Consumer: Process messages from channel
        var consumerTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var message in _messageReader.ReadAllAsync(combinedCts.Token))
                {
                    try
                    {
                        _logger?.LogTrace("Consumer processing message: {Message}", message);
                        MessageReceived?.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error processing message: {Message}", message);
                        ProcessingError?.Invoke(ex);
                    }
                }
            }
            catch (OperationCanceledException) when (combinedCts.Token.IsCancellationRequested)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in message consumer");
                ProcessingError?.Invoke(ex);
            }
            finally
            {
                _logger?.LogDebug("Message consumer completed");
            }
        }, combinedCts.Token);

        // Health monitoring task
        var healthMonitorTask = Task.Run(async () =>
        {
            var lastHealthStatus = true;
            
            while (!combinedCts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), combinedCts.Token);
                    
                    var timeSinceLastMessage = DateTime.UtcNow - _lastMessageReceived;
                    var isHealthy = timeSinceLastMessage < TimeSpan.FromSeconds(30);
                    
                    if (isHealthy != lastHealthStatus)
                    {
                        lastHealthStatus = isHealthy;
                        ConnectionHealthChanged?.Invoke(isHealthy);
                        
                        if (isHealthy)
                        {
                            _logger?.LogInformation("Connection health restored");
                        }
                        else
                        {
                            _logger?.LogWarning("Connection health degraded - no messages received for {Duration}s", 
                                timeSinceLastMessage.TotalSeconds);
                        }
                    }
                }
                catch (OperationCanceledException) when (combinedCts.Token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error in health monitor");
                }
            }
        }, combinedCts.Token);

        // Store the combined task for cleanup
        _messageProcessingTask = Task.WhenAll(producerTask, consumerTask, healthMonitorTask);

        try
        {
            await _messageProcessingTask;
        }
        catch (OperationCanceledException) when (combinedCts.Token.IsCancellationRequested)
        {
            _logger?.LogInformation("Message processing stopped due to cancellation");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Message processing pipeline failed");
            throw;
        }
        finally
        {
            _messageProcessingTask = null;
        }
    }

    /// <summary>
    /// Stops the message processing pipeline gracefully.
    /// </summary>
    /// <returns>Task representing the stop operation.</returns>
    public async Task StopMessageProcessingAsync()
    {
        if (_messageProcessingTask == null)
        {
            _logger?.LogDebug("Message processing is not running");
            return;
        }

        _logger?.LogInformation("Stopping message processing pipeline");
        
        _processingCts.Cancel();
        
        try
        {
            await _messageProcessingTask;
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        
        _logger?.LogInformation("Message processing pipeline stopped");
    }

    /// <summary>
    /// Gets the current health status of the connection.
    /// </summary>
    /// <returns>True if the connection is healthy, false otherwise.</returns>
    public bool IsHealthy()
    {
        var timeSinceLastMessage = DateTime.UtcNow - _lastMessageReceived;
        return timeSinceLastMessage < TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Gets statistics about the message processing pipeline.
    /// </summary>
    /// <returns>Processing statistics.</returns>
    public MessageProcessingStats GetProcessingStats()
    {
        return new MessageProcessingStats
        {
            IsProcessing = _messageProcessingTask != null && !_messageProcessingTask.IsCompleted,
            LastMessageReceived = _lastMessageReceived,
            IsHealthy = IsHealthy(),
            QueuedMessages = _messageReader.CanCount ? _messageReader.Count : -1
        };
    }

    /// <summary>
    /// Gets statistics about the circuit breaker operation (Phase 3).
    /// </summary>
    /// <returns>Circuit breaker statistics.</returns>
    public CircuitBreakerStats GetCircuitBreakerStats()
    {
        return _circuitBreaker.Stats;
    }

    /// <summary>
    /// Gets the current state of the circuit breaker (Phase 3).
    /// </summary>
    /// <returns>Current circuit breaker state.</returns>
    public CircuitBreakerState GetCircuitBreakerState()
    {
        return _circuitBreaker.State;
    }

    /// <summary>
    /// Manually resets the circuit breaker to closed state (Phase 3).
    /// </summary>
    public void ResetCircuitBreaker()
    {
        _circuitBreaker.Reset();
        _logger?.LogInformation("Circuit breaker manually reset");
    }

    /// <summary>
    /// Gets comprehensive connection diagnostics including health and circuit breaker status (Phase 3).
    /// </summary>
    /// <returns>Connection diagnostics information.</returns>
    public ConnectionDiagnostics GetDiagnostics()
    {
        var processingStats = GetProcessingStats();
        var circuitBreakerStats = GetCircuitBreakerStats();
        var healthStats = _healthMonitor.GetHealthStats();

        return new ConnectionDiagnostics
        {
            ProcessingStats = processingStats,
            CircuitBreakerStats = circuitBreakerStats,
            HealthStats = healthStats,
            OverallHealth = processingStats.IsHealthy && circuitBreakerStats.State == CircuitBreakerState.Closed && healthStats.IsHealthy
        };
    }

    /// <summary>
    /// Handles circuit breaker state changes (Phase 3).
    /// </summary>
    private void OnCircuitBreakerStateChanged(CircuitBreakerState oldState, CircuitBreakerState newState)
    {
        _logger?.LogInformation("Circuit breaker state changed from {OldState} to {NewState}", oldState, newState);
        CircuitBreakerStateChanged?.Invoke(oldState, newState);
        
        // Notify health change when circuit breaker opens/closes
        var isHealthy = newState == CircuitBreakerState.Closed;
        ConnectionHealthChanged?.Invoke(isHealthy);
    }

    /// <summary>
    /// Handles health monitor status changes (Phase 3).
    /// </summary>
    private void OnHealthMonitorStatusChanged(bool isHealthy)
    {
        _logger?.LogDebug("Health monitor status changed to: {IsHealthy}", isHealthy);
        ConnectionHealthChanged?.Invoke(isHealthy);
    }

    /// <summary>
    /// Releases all resources used by the TcpConnection.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        
        // Stop message processing pipeline
        _processingCts.Cancel();
        
        try
        {
            // Wait for processing to complete with timeout
            _messageProcessingTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error while stopping message processing during disposal");
        }

        // Dispose Phase 3 resources
        try
        {
            _healthMonitor?.Dispose();
            _circuitBreaker.StateChanged -= OnCircuitBreakerStateChanged;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error while disposing Phase 3 resources");
        }
        
        // Dispose core resources
        _processingCts.Dispose();
        Stream?.Dispose();
        Client?.Dispose();
        
        _logger?.LogDebug("TcpConnection disposed with circuit breaker and health monitoring");
    }
}
