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
/// Configuration options for the Snapcast client
/// </summary>
public class SnapcastClientOptions
{
    /// <summary>
    /// Connection timeout in milliseconds. Default: 5000ms (5 seconds)
    /// </summary>
    public int ConnectionTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Maximum number of retry attempts for failed operations. Default: 3
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Interval between health checks in milliseconds. Default: 30000ms (30 seconds)
    /// </summary>
    public int HealthCheckIntervalMs { get; set; } = 30000;

    /// <summary>
    /// Whether to include stack traces in connection error logs.
    /// When false, only the error message is logged. When true, full stack traces are included.
    /// Default: false (clean logging)
    /// </summary>
    public bool VerboseConnectionLogging { get; set; } = false;

    /// <summary>
    /// Enable automatic reconnection on connection loss. Default: true
    /// </summary>
    public bool EnableAutoReconnect { get; set; } = true;

    /// <summary>
    /// Delay between reconnection attempts in milliseconds. Default: 1000ms (1 second)
    /// </summary>
    public int ReconnectDelayMs { get; set; } = 1000;

    /// <summary>
    /// Maximum delay between reconnection attempts in milliseconds. Default: 30000ms (30 seconds)
    /// </summary>
    public int MaxReconnectDelayMs { get; set; } = 30000;

    /// <summary>
    /// Enable exponential backoff for reconnection attempts. Default: true
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Enable detailed logging of all operations. Default: false
    /// </summary>
    public bool EnableVerboseLogging { get; set; } = false;

    /// <summary>
    /// Buffer size for network operations in bytes. Default: 1024
    /// </summary>
    public int BufferSize { get; set; } = 1024;
}
