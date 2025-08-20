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
/// Statistics about the message processing pipeline performance and health.
/// </summary>
public class MessageProcessingStats
{
    /// <summary>
    /// Gets or sets whether the message processing pipeline is currently running.
    /// </summary>
    public bool IsProcessing { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the last message was received.
    /// </summary>
    public DateTime LastMessageReceived { get; set; }

    /// <summary>
    /// Gets or sets whether the connection is considered healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the number of messages currently queued for processing.
    /// Returns -1 if the count is not available.
    /// </summary>
    public int QueuedMessages { get; set; }

    /// <summary>
    /// Gets the time elapsed since the last message was received.
    /// </summary>
    public TimeSpan TimeSinceLastMessage => DateTime.UtcNow - LastMessageReceived;

    /// <summary>
    /// Returns a string representation of the processing statistics.
    /// </summary>
    /// <returns>A formatted string with key statistics.</returns>
    public override string ToString()
    {
        return $"Processing: {IsProcessing}, Healthy: {IsHealthy}, " +
               $"Last Message: {TimeSinceLastMessage.TotalSeconds:F1}s ago, " +
               $"Queued: {(QueuedMessages >= 0 ? QueuedMessages.ToString() : "N/A")}";
    }
}
