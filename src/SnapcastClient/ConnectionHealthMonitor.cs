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
/// Advanced connection health monitoring with configurable thresholds and metrics.
/// Provides real-time health assessment and trend analysis for network connections.
/// </summary>
public class ConnectionHealthMonitor : IDisposable
{
    private readonly ILogger<ConnectionHealthMonitor>? _logger;
    private readonly ConnectionHealthOptions _options;
    private readonly Timer _healthCheckTimer;
    private readonly object _lock = new();
    
    private DateTime _lastMessageReceived = DateTime.UtcNow;
    private DateTime _lastHealthCheck = DateTime.UtcNow;
    private bool _isHealthy = true;
    private bool _disposed = false;
    
    // Health metrics
    private int _totalMessages = 0;
    private int _healthyChecks = 0;
    private int _unhealthyChecks = 0;
    private TimeSpan _totalDowntime = TimeSpan.Zero;
    private DateTime? _currentDowntimeStart = null;

    /// <summary>
    /// Event fired when the connection health status changes.
    /// </summary>
    public event Action<bool>? ConnectionHealthChanged;

    /// <summary>
    /// Initializes a new instance of the ConnectionHealthMonitor class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="options">Optional health monitoring configuration.</param>
    public ConnectionHealthMonitor(ILogger? logger = null, ConnectionHealthOptions? options = null)
    {
        _logger = logger as ILogger<ConnectionHealthMonitor>;
        _options = options ?? new ConnectionHealthOptions();
        
        _healthCheckTimer = new Timer(PerformHealthCheck, null, 
            _options.CheckInterval, _options.CheckInterval);
        
        _logger?.LogDebug("Connection health monitor initialized with check interval: {CheckInterval}ms, timeout: {Timeout}ms", 
            _options.CheckInterval.TotalMilliseconds, _options.HealthyThreshold.TotalMilliseconds);
    }

    /// <summary>
    /// Records that a message was received, updating health metrics.
    /// </summary>
    public void RecordMessageReceived()
    {
        if (_disposed) return;

        lock (_lock)
        {
            _lastMessageReceived = DateTime.UtcNow;
            _totalMessages++;
            
            _logger?.LogTrace("Message received recorded. Total messages: {TotalMessages}", _totalMessages);
        }
    }

    /// <summary>
    /// Gets the current health status of the connection.
    /// </summary>
    /// <returns>True if the connection is healthy, false otherwise.</returns>
    public bool IsHealthy()
    {
        if (_disposed) return false;

        lock (_lock)
        {
            return _isHealthy;
        }
    }

    /// <summary>
    /// Gets comprehensive health statistics.
    /// </summary>
    /// <returns>Health statistics including metrics and trends.</returns>
    public ConnectionHealthStats GetHealthStats()
    {
        if (_disposed)
        {
            return new ConnectionHealthStats
            {
                IsHealthy = false,
                LastMessageReceived = DateTime.MinValue,
                TimeSinceLastMessage = TimeSpan.MaxValue,
                TotalMessages = 0,
                HealthyChecks = 0,
                UnhealthyChecks = 0,
                TotalDowntime = TimeSpan.MaxValue,
                UptimePercentage = 0.0,
                IsDisposed = true
            };
        }

        lock (_lock)
        {
            var timeSinceLastMessage = DateTime.UtcNow - _lastMessageReceived;
            var totalChecks = _healthyChecks + _unhealthyChecks;
            var uptimePercentage = totalChecks > 0 ? (_healthyChecks / (double)totalChecks) * 100.0 : 100.0;
            
            // Calculate current downtime if unhealthy
            var currentDowntime = _totalDowntime;
            if (!_isHealthy && _currentDowntimeStart.HasValue)
            {
                currentDowntime += DateTime.UtcNow - _currentDowntimeStart.Value;
            }

            return new ConnectionHealthStats
            {
                IsHealthy = _isHealthy,
                LastMessageReceived = _lastMessageReceived,
                TimeSinceLastMessage = timeSinceLastMessage,
                TotalMessages = _totalMessages,
                HealthyChecks = _healthyChecks,
                UnhealthyChecks = _unhealthyChecks,
                TotalDowntime = currentDowntime,
                UptimePercentage = uptimePercentage,
                IsDisposed = false
            };
        }
    }

    /// <summary>
    /// Manually triggers a health check.
    /// </summary>
    /// <returns>Current health status after the check.</returns>
    public bool PerformHealthCheckNow()
    {
        if (_disposed) return false;

        PerformHealthCheck(null);
        return IsHealthy();
    }

    /// <summary>
    /// Resets all health statistics and sets status to healthy.
    /// </summary>
    public void Reset()
    {
        if (_disposed) return;

        lock (_lock)
        {
            _lastMessageReceived = DateTime.UtcNow;
            _lastHealthCheck = DateTime.UtcNow;
            _isHealthy = true;
            _totalMessages = 0;
            _healthyChecks = 0;
            _unhealthyChecks = 0;
            _totalDowntime = TimeSpan.Zero;
            _currentDowntimeStart = null;
            
            _logger?.LogInformation("Connection health monitor reset");
        }
    }

    private void PerformHealthCheck(object? state)
    {
        if (_disposed) return;

        try
        {
            bool newHealthStatus;
            
            lock (_lock)
            {
                var timeSinceLastMessage = DateTime.UtcNow - _lastMessageReceived;
                newHealthStatus = timeSinceLastMessage <= _options.HealthyThreshold;
                
                _lastHealthCheck = DateTime.UtcNow;
                
                // Update health check counters
                if (newHealthStatus)
                {
                    _healthyChecks++;
                }
                else
                {
                    _unhealthyChecks++;
                }
                
                // Handle health status changes
                if (_isHealthy != newHealthStatus)
                {
                    var oldStatus = _isHealthy;
                    _isHealthy = newHealthStatus;
                    
                    if (newHealthStatus)
                    {
                        // Became healthy - record downtime
                        if (_currentDowntimeStart.HasValue)
                        {
                            _totalDowntime += DateTime.UtcNow - _currentDowntimeStart.Value;
                            _currentDowntimeStart = null;
                        }
                        
                        _logger?.LogInformation("Connection health restored. Time since last message: {TimeSinceLastMessage}s", 
                            timeSinceLastMessage.TotalSeconds);
                    }
                    else
                    {
                        // Became unhealthy - start tracking downtime
                        _currentDowntimeStart = DateTime.UtcNow;
                        
                        _logger?.LogWarning("Connection health degraded. Time since last message: {TimeSinceLastMessage}s (threshold: {Threshold}s)", 
                            timeSinceLastMessage.TotalSeconds, _options.HealthyThreshold.TotalSeconds);
                    }
                    
                    // Fire event outside of lock
                    Task.Run(() => ConnectionHealthChanged?.Invoke(newHealthStatus));
                }
                else if (!newHealthStatus)
                {
                    // Still unhealthy - log periodic warnings
                    if (_unhealthyChecks % 6 == 0) // Every minute with 10s intervals
                    {
                        _logger?.LogWarning("Connection remains unhealthy. Time since last message: {TimeSinceLastMessage}s", 
                            timeSinceLastMessage.TotalSeconds);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during health check");
        }
    }

    /// <summary>
    /// Releases all resources used by the ConnectionHealthMonitor.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        
        try
        {
            _healthCheckTimer?.Dispose();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error disposing health check timer");
        }
        
        _logger?.LogDebug("Connection health monitor disposed");
    }
}

/// <summary>
/// Configuration options for connection health monitoring.
/// </summary>
public class ConnectionHealthOptions
{
    /// <summary>
    /// Interval between health checks.
    /// Default: 10 seconds
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Maximum time since last message before connection is considered unhealthy.
    /// Default: 30 seconds
    /// </summary>
    public TimeSpan HealthyThreshold { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Comprehensive health statistics for connection monitoring.
/// </summary>
public class ConnectionHealthStats
{
    /// <summary>
    /// Current health status of the connection.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Timestamp when the last message was received.
    /// </summary>
    public DateTime LastMessageReceived { get; set; }

    /// <summary>
    /// Time elapsed since the last message was received.
    /// </summary>
    public TimeSpan TimeSinceLastMessage { get; set; }

    /// <summary>
    /// Total number of messages received.
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// Number of health checks that passed.
    /// </summary>
    public int HealthyChecks { get; set; }

    /// <summary>
    /// Number of health checks that failed.
    /// </summary>
    public int UnhealthyChecks { get; set; }

    /// <summary>
    /// Total time the connection has been unhealthy.
    /// </summary>
    public TimeSpan TotalDowntime { get; set; }

    /// <summary>
    /// Percentage of time the connection has been healthy.
    /// </summary>
    public double UptimePercentage { get; set; }

    /// <summary>
    /// Whether the health monitor has been disposed.
    /// </summary>
    public bool IsDisposed { get; set; }

    /// <summary>
    /// Total number of health checks performed.
    /// </summary>
    public int TotalChecks => HealthyChecks + UnhealthyChecks;

    /// <summary>
    /// Returns a formatted string representation of the health statistics.
    /// </summary>
    public override string ToString()
    {
        if (IsDisposed)
            return "Health Monitor: Disposed";

        return $"Health: {(IsHealthy ? "Healthy" : "Unhealthy")}, " +
               $"Messages: {TotalMessages}, " +
               $"Uptime: {UptimePercentage:F1}%, " +
               $"Last Message: {TimeSinceLastMessage.TotalSeconds:F1}s ago, " +
               $"Total Downtime: {TotalDowntime.TotalSeconds:F1}s";
    }
}

/// <summary>
/// Comprehensive connection diagnostics combining all monitoring aspects.
/// </summary>
public class ConnectionDiagnostics
{
    /// <summary>
    /// Message processing statistics.
    /// </summary>
    public MessageProcessingStats ProcessingStats { get; set; } = new();

    /// <summary>
    /// Circuit breaker statistics.
    /// </summary>
    public CircuitBreakerStats CircuitBreakerStats { get; set; } = new();

    /// <summary>
    /// Health monitoring statistics.
    /// </summary>
    public ConnectionHealthStats HealthStats { get; set; } = new();

    /// <summary>
    /// Overall health assessment combining all factors.
    /// </summary>
    public bool OverallHealth { get; set; }

    /// <summary>
    /// Returns a comprehensive diagnostic summary.
    /// </summary>
    public override string ToString()
    {
        return $"Overall Health: {(OverallHealth ? "Healthy" : "Unhealthy")}\n" +
               $"Processing: {ProcessingStats}\n" +
               $"Circuit Breaker: {CircuitBreakerStats}\n" +
               $"Health Monitor: {HealthStats}";
    }
}
