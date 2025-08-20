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

namespace SnapcastClient;

/// <summary>
/// Interface for network connections to the Snapcast server.
/// </summary>
public interface IConnection
{
    /// <summary>
    /// Sends data to the server.
    /// </summary>
    /// <param name="data">The data to send.</param>
    public void Send(string data);

    /// <summary>
    /// Reads data from the server (legacy synchronous method).
    /// </summary>
    /// <returns>The data received from the server, or null if no data is available.</returns>
    [Obsolete("Use ReadAsync for better performance and timeout handling")]
    public string? Read();

    /// <summary>
    /// Enterprise-grade async method to read data from the server with timeout and cancellation support.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The complete JSON message received from the server, or null if no data is available.</returns>
    public Task<string?> ReadAsync(CancellationToken cancellationToken = default);

    // Producer-Consumer Pattern (Phase 2)
    
    /// <summary>
    /// Event fired when a complete message is received and processed.
    /// </summary>
    public event Action<string>? MessageReceived;

    /// <summary>
    /// Event fired when an error occurs during message processing.
    /// </summary>
    public event Action<Exception>? ProcessingError;

    /// <summary>
    /// Event fired when the connection health status changes.
    /// </summary>
    public event Action<bool>? ConnectionHealthChanged;

    /// <summary>
    /// Event fired when the circuit breaker state changes.
    /// </summary>
    public event Action<CircuitBreakerState, CircuitBreakerState>? CircuitBreakerStateChanged;

    /// <summary>
    /// Starts the producer-consumer message processing pipeline.
    /// This enables concurrent message processing with automatic error recovery.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop processing.</param>
    /// <returns>Task representing the message processing operation.</returns>
    public Task StartMessageProcessingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the message processing pipeline gracefully.
    /// </summary>
    /// <returns>Task representing the stop operation.</returns>
    public Task StopMessageProcessingAsync();

    /// <summary>
    /// Gets the current health status of the connection.
    /// </summary>
    /// <returns>True if the connection is healthy, false otherwise.</returns>
    public bool IsHealthy();

    /// <summary>
    /// Gets statistics about the message processing pipeline.
    /// </summary>
    /// <returns>Processing statistics.</returns>
    public MessageProcessingStats GetProcessingStats();

    // Circuit Breaker Pattern (Phase 3)

    /// <summary>
    /// Gets statistics about the circuit breaker operation.
    /// </summary>
    /// <returns>Circuit breaker statistics.</returns>
    public CircuitBreakerStats GetCircuitBreakerStats();

    /// <summary>
    /// Gets the current state of the circuit breaker.
    /// </summary>
    /// <returns>Current circuit breaker state.</returns>
    public CircuitBreakerState GetCircuitBreakerState();

    /// <summary>
    /// Manually resets the circuit breaker to closed state.
    /// </summary>
    public void ResetCircuitBreaker();

    /// <summary>
    /// Gets comprehensive connection diagnostics including health and circuit breaker status.
    /// </summary>
    /// <returns>Connection diagnostics information.</returns>
    public ConnectionDiagnostics GetDiagnostics();
}
