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

using Microsoft.Extensions.Logging;

namespace SnapcastClient;

/// <summary>
/// Enterprise-grade circuit breaker implementation for resilient network operations.
/// Prevents cascading failures by temporarily stopping operations when failure rate exceeds threshold.
/// </summary>
public class CircuitBreaker
{
    private readonly ILogger<CircuitBreaker>? _logger;
    private readonly CircuitBreakerOptions _options;
    private readonly object _lock = new();
    
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _failureCount = 0;
    private int _successCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private DateTime _lastStateChangeTime = DateTime.UtcNow;

    /// <summary>
    /// Gets the current state of the circuit breaker.
    /// </summary>
    public CircuitBreakerState State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }

    /// <summary>
    /// Gets statistics about the circuit breaker's operation.
    /// </summary>
    public CircuitBreakerStats Stats
    {
        get
        {
            lock (_lock)
            {
                return new CircuitBreakerStats
                {
                    State = _state,
                    FailureCount = _failureCount,
                    SuccessCount = _successCount,
                    LastFailureTime = _lastFailureTime,
                    LastStateChangeTime = _lastStateChangeTime,
                    TimeSinceLastFailure = _lastFailureTime == DateTime.MinValue 
                        ? TimeSpan.Zero 
                        : DateTime.UtcNow - _lastFailureTime,
                    TimeInCurrentState = DateTime.UtcNow - _lastStateChangeTime
                };
            }
        }
    }

    /// <summary>
    /// Event fired when the circuit breaker state changes.
    /// </summary>
    public event Action<CircuitBreakerState, CircuitBreakerState>? StateChanged;

    /// <summary>
    /// Initializes a new instance of the CircuitBreaker class.
    /// </summary>
    /// <param name="options">Configuration options for the circuit breaker.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public CircuitBreaker(CircuitBreakerOptions? options = null, ILogger? logger = null)
    {
        _options = options ?? new CircuitBreakerOptions();
        _logger = logger as ILogger<CircuitBreaker>;
        
        _logger?.LogDebug("Circuit breaker initialized with failure threshold: {FailureThreshold}, timeout: {Timeout}ms", 
            _options.FailureThreshold, _options.Timeout.TotalMilliseconds);
    }

    /// <summary>
    /// Executes an operation through the circuit breaker with resilience protection.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="CircuitBreakerOpenException">Thrown when the circuit breaker is open.</exception>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        // Check if we should allow the operation
        if (!ShouldAllowExecution())
        {
            var stats = Stats;
            _logger?.LogWarning("Circuit breaker is open - rejecting operation. Failures: {FailureCount}, Time since last failure: {TimeSinceLastFailure}s", 
                stats.FailureCount, stats.TimeSinceLastFailure.TotalSeconds);
            throw new CircuitBreakerOpenException($"Circuit breaker is open. Failure count: {stats.FailureCount}, Time in open state: {stats.TimeInCurrentState.TotalSeconds:F1}s");
        }

        try
        {
            _logger?.LogTrace("Executing operation through circuit breaker");
            var result = await operation();
            
            OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an operation through the circuit breaker without return value.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <exception cref="CircuitBreakerOpenException">Thrown when the circuit breaker is open.</exception>
    public async Task ExecuteAsync(Func<Task> operation)
    {
        await ExecuteAsync(async () =>
        {
            await operation();
            return true; // Dummy return value
        });
    }

    /// <summary>
    /// Manually resets the circuit breaker to closed state.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            var oldState = _state;
            _state = CircuitBreakerState.Closed;
            _failureCount = 0;
            _successCount = 0;
            _lastFailureTime = DateTime.MinValue;
            _lastStateChangeTime = DateTime.UtcNow;
            
            _logger?.LogInformation("Circuit breaker manually reset to closed state");
            
            if (oldState != _state)
            {
                StateChanged?.Invoke(oldState, _state);
            }
        }
    }

    private bool ShouldAllowExecution()
    {
        lock (_lock)
        {
            switch (_state)
            {
                case CircuitBreakerState.Closed:
                    return true;

                case CircuitBreakerState.Open:
                    // Check if timeout period has elapsed
                    if (DateTime.UtcNow - _lastStateChangeTime >= _options.Timeout)
                    {
                        TransitionToHalfOpen();
                        return true;
                    }
                    return false;

                case CircuitBreakerState.HalfOpen:
                    return true;

                default:
                    return false;
            }
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            _successCount++;
            
            switch (_state)
            {
                case CircuitBreakerState.HalfOpen:
                    // After successful operation in half-open state, close the circuit
                    if (_successCount >= _options.SuccessThreshold)
                    {
                        TransitionToClosed();
                    }
                    break;

                case CircuitBreakerState.Closed:
                    // Reset failure count on success
                    if (_failureCount > 0)
                    {
                        _logger?.LogDebug("Resetting failure count after successful operation");
                        _failureCount = 0;
                    }
                    break;
            }
        }
    }

    private void OnFailure(Exception exception)
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            _logger?.LogWarning(exception, "Operation failed through circuit breaker. Failure count: {FailureCount}", _failureCount);

            switch (_state)
            {
                case CircuitBreakerState.Closed:
                    if (_failureCount >= _options.FailureThreshold)
                    {
                        TransitionToOpen();
                    }
                    break;

                case CircuitBreakerState.HalfOpen:
                    // Any failure in half-open state immediately opens the circuit
                    TransitionToOpen();
                    break;
            }
        }
    }

    private void TransitionToOpen()
    {
        var oldState = _state;
        _state = CircuitBreakerState.Open;
        _lastStateChangeTime = DateTime.UtcNow;
        
        _logger?.LogWarning("Circuit breaker opened due to {FailureCount} failures. Will retry after {Timeout}ms", 
            _failureCount, _options.Timeout.TotalMilliseconds);
        
        StateChanged?.Invoke(oldState, _state);
    }

    private void TransitionToHalfOpen()
    {
        var oldState = _state;
        _state = CircuitBreakerState.HalfOpen;
        _successCount = 0; // Reset success count for half-open evaluation
        _lastStateChangeTime = DateTime.UtcNow;
        
        _logger?.LogInformation("Circuit breaker transitioning to half-open state for testing");
        
        StateChanged?.Invoke(oldState, _state);
    }

    private void TransitionToClosed()
    {
        var oldState = _state;
        _state = CircuitBreakerState.Closed;
        _failureCount = 0;
        _successCount = 0;
        _lastStateChangeTime = DateTime.UtcNow;
        
        _logger?.LogInformation("Circuit breaker closed - service recovered");
        
        StateChanged?.Invoke(oldState, _state);
    }
}

/// <summary>
/// Configuration options for the circuit breaker.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Number of consecutive failures required to open the circuit breaker.
    /// Default: 5
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Time to wait before transitioning from open to half-open state.
    /// Default: 60 seconds
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Number of consecutive successes required to close the circuit breaker from half-open state.
    /// Default: 3
    /// </summary>
    public int SuccessThreshold { get; set; } = 3;
}

/// <summary>
/// Represents the state of a circuit breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Circuit breaker is closed - operations are allowed.
    /// </summary>
    Closed,

    /// <summary>
    /// Circuit breaker is open - operations are blocked.
    /// </summary>
    Open,

    /// <summary>
    /// Circuit breaker is half-open - testing if service has recovered.
    /// </summary>
    HalfOpen
}

/// <summary>
/// Statistics about circuit breaker operation.
/// </summary>
public class CircuitBreakerStats
{
    /// <summary>
    /// Current state of the circuit breaker.
    /// </summary>
    public CircuitBreakerState State { get; set; }

    /// <summary>
    /// Number of consecutive failures.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Number of consecutive successes (in half-open state).
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Timestamp of the last failure.
    /// </summary>
    public DateTime LastFailureTime { get; set; }

    /// <summary>
    /// Timestamp when the circuit breaker last changed state.
    /// </summary>
    public DateTime LastStateChangeTime { get; set; }

    /// <summary>
    /// Time elapsed since the last failure.
    /// </summary>
    public TimeSpan TimeSinceLastFailure { get; set; }

    /// <summary>
    /// Time elapsed in the current state.
    /// </summary>
    public TimeSpan TimeInCurrentState { get; set; }

    /// <summary>
    /// Returns a formatted string representation of the circuit breaker statistics.
    /// </summary>
    public override string ToString()
    {
        return $"State: {State}, Failures: {FailureCount}, Successes: {SuccessCount}, " +
               $"Time in state: {TimeInCurrentState.TotalSeconds:F1}s, " +
               $"Time since last failure: {TimeSinceLastFailure.TotalSeconds:F1}s";
    }
}

/// <summary>
/// Exception thrown when the circuit breaker is open and operations are not allowed.
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the CircuitBreakerOpenException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CircuitBreakerOpenException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the CircuitBreakerOpenException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public CircuitBreakerOpenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
