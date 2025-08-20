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
using Moq;
using NUnit.Framework;

namespace SnapcastClient.Tests;

/// <summary>
/// Tests for Phase 2 producer-consumer pattern implementation.
/// Demonstrates the enterprise-grade concurrent message processing capabilities.
/// </summary>
[TestFixture]
public class ProducerConsumerTests
{
    [Test]
    public void IConnection_ShouldHaveProducerConsumerEvents()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();

        // Act & Assert - Verify Phase 2 events are available in interface
        Assert.That(typeof(IConnection).GetEvent("MessageReceived"), Is.Not.Null);
        Assert.That(typeof(IConnection).GetEvent("ProcessingError"), Is.Not.Null);
        Assert.That(typeof(IConnection).GetEvent("ConnectionHealthChanged"), Is.Not.Null);
    }

    [Test]
    public void IConnection_ShouldHaveProducerConsumerMethods()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();

        // Act & Assert - Verify Phase 2 methods are available in interface
        Assert.That(typeof(IConnection).GetMethod("StartMessageProcessingAsync"), Is.Not.Null);
        Assert.That(typeof(IConnection).GetMethod("StopMessageProcessingAsync"), Is.Not.Null);
        Assert.That(typeof(IConnection).GetMethod("IsHealthy"), Is.Not.Null);
        Assert.That(typeof(IConnection).GetMethod("GetProcessingStats"), Is.Not.Null);
    }

    [Test]
    public void MessageProcessingStats_ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var stats = new MessageProcessingStats
        {
            IsProcessing = true,
            IsHealthy = true,
            LastMessageReceived = DateTime.UtcNow.AddSeconds(-5),
            QueuedMessages = 3
        };

        // Act
        var result = stats.ToString();

        // Assert
        Assert.That(result, Does.Contain("Processing: True"));
        Assert.That(result, Does.Contain("Healthy: True"));
        Assert.That(result, Does.Contain("Queued: 3"));
    }

    [Test]
    public void MessageProcessingStats_TimeSinceLastMessage_ShouldCalculateCorrectly()
    {
        // Arrange
        var fiveSecondsAgo = DateTime.UtcNow.AddSeconds(-5);
        var stats = new MessageProcessingStats
        {
            LastMessageReceived = fiveSecondsAgo
        };

        // Act
        var timeSince = stats.TimeSinceLastMessage;

        // Assert
        Assert.That(timeSince.TotalSeconds, Is.GreaterThanOrEqualTo(4.5));
        Assert.That(timeSince.TotalSeconds, Is.LessThanOrEqualTo(5.5));
    }

    [Test]
    public void MessageProcessingStats_DefaultValues_ShouldBeValid()
    {
        // Arrange & Act
        var stats = new MessageProcessingStats();

        // Assert
        Assert.That(stats.IsProcessing, Is.False);
        Assert.That(stats.IsHealthy, Is.False);
        Assert.That(stats.QueuedMessages, Is.EqualTo(0));
        Assert.That(stats.LastMessageReceived, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public void MessageProcessingStats_QueuedMessages_ShouldHandleNegativeValues()
    {
        // Arrange
        var stats = new MessageProcessingStats
        {
            QueuedMessages = -1
        };

        // Act
        var result = stats.ToString();

        // Assert
        Assert.That(result, Does.Contain("Queued: N/A"));
    }
}
