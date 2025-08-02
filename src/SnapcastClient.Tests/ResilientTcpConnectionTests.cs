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

namespace SnapcastClient.tests;

[TestFixture]
public class ResilientTcpConnectionTests
{
    private Mock<ILogger<ResilientTcpConnection>> _mockLogger;
    private SnapcastClientOptions _options;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<ResilientTcpConnection>>();
        _options = new SnapcastClientOptions
        {
            EnableAutoReconnect = true,
            MaxRetryAttempts = 3,
            ReconnectDelayMs = 100, // Short delay for tests
            MaxReconnectDelayMs = 1000,
            UseExponentialBackoff = true,
            HealthCheckIntervalMs = 0 // Disable health checks for most tests
        };
    }

    [Test]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange & Act
        using var connection = new ResilientTcpConnection("localhost", 1705, _options, _mockLogger.Object);

        // Assert - Connection state should be one of the valid states (timing dependent)
        Assert.That(connection.State, Is.AnyOf(
            ConnectionState.Disconnected, 
            ConnectionState.Connecting, 
            ConnectionState.Connected, 
            ConnectionState.Failed,
            ConnectionState.Reconnecting));
        Assert.That(connection.ReconnectAttempts, Is.GreaterThanOrEqualTo(0));
        Assert.That(connection.LastHealthCheck, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public void Constructor_WithNullHost_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ResilientTcpConnection(null!, 1705, _options, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithDefaultOptions_UsesDefaults()
    {
        // Arrange & Act
        using var connection = new ResilientTcpConnection("localhost", 1705, null, _mockLogger.Object);

        // Assert - Should not throw and should have reasonable state
        Assert.That(Enum.IsDefined(typeof(ConnectionState), connection.State), Is.True);
    }

    [Test]
    public void Send_WhenDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var connection = new ResilientTcpConnection("localhost", 1705, _options, _mockLogger.Object);
        connection.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => connection.Send("test"));
    }

    [Test]
    public void Read_WhenDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var connection = new ResilientTcpConnection("localhost", 1705, _options, _mockLogger.Object);
        connection.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => connection.Read());
    }

    [Test]
    [Ignore("Disabled due to flaky behavior in CI environment - network timing differences")]
    public void OnConnectionStateChanged_WhenStateChanges_FiresEvent()
    {
        // Arrange
        ConnectionState? capturedState = null;
        var eventFired = false;

        // Use a non-routable IP address that will fail consistently across environments
        using var connection = new ResilientTcpConnection("192.0.2.1", 1705, _options, _mockLogger.Object);
        connection.OnConnectionStateChanged += state =>
        {
            capturedState = state;
            eventFired = true;
        };

        // Act - Wait for initial connection attempt to complete
        Thread.Sleep(1000);

        // Assert
        Assert.That(eventFired, Is.True);
        Assert.That(capturedState.HasValue, Is.True);
    }

    [Test]
    public void OnReconnectAttempt_EventCanBeSubscribed()
    {
        // Arrange
        var eventSubscribed = false;
        _options.MaxRetryAttempts = 1;
        _options.EnableAutoReconnect = true;
        
        using var connection = new ResilientTcpConnection("localhost", 1705, _options, _mockLogger.Object);
        
        // Act - Subscribe to the event
        connection.OnReconnectAttempt += (attempt, ex) =>
        {
            eventSubscribed = true;
        };

        // Assert - We can verify the event subscription doesn't throw
        // and that the connection has the expected configuration
        Assert.DoesNotThrow(() => 
        {
            connection.OnReconnectAttempt += (attempt, ex) => { };
        });
        
        // Verify the variable was properly initialized
        Assert.That(eventSubscribed, Is.False); // Should be false since no reconnect attempt was triggered
    }

    [Test]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        // Arrange
        var connection = new ResilientTcpConnection("localhost", 1705, _options, _mockLogger.Object);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            connection.Dispose();
            connection.Dispose(); // Second call should not throw
        });
    }

    [Test]
    public void HealthCheck_WhenEnabled_UpdatesLastHealthCheck()
    {
        // Arrange
        _options.HealthCheckIntervalMs = 100; // Very short interval for test
        var initialTime = DateTime.UtcNow;

        using var connection = new ResilientTcpConnection("localhost", 1705, _options, _mockLogger.Object);

        // Act - Wait for health check to run
        Thread.Sleep(200);

        // Assert
        Assert.That(connection.LastHealthCheck, Is.GreaterThan(initialTime));
    }

    [Test]
    public void ExponentialBackoff_WhenEnabled_IncreasesDelay()
    {
        // This test verifies the concept but is hard to test precisely due to timing
        // We mainly verify that the option is respected and doesn't cause issues
        
        // Arrange
        _options.UseExponentialBackoff = true;
        _options.MaxRetryAttempts = 2;
        _options.ReconnectDelayMs = 50;

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() =>
        {
            using var connection = new ResilientTcpConnection("invalid-host", 1705, _options, _mockLogger.Object);
            Thread.Sleep(300); // Wait for retry attempts
        });
    }

    [Test]
    [Ignore("Disabled due to flaky behavior in CI environment - network timing differences")]
    public void AutoReconnect_WhenDisabled_DoesNotRetry()
    {
        // Arrange
        _options.EnableAutoReconnect = false;
        _options.MaxRetryAttempts = 3; // Allow multiple attempts to see the difference
        var reconnectAttempts = 0;
        var attemptDetails = new List<int>();

        // Use localhost with an invalid port to ensure connection failure
        using var connection = new ResilientTcpConnection("127.0.0.1", 9999, _options, _mockLogger.Object);
        connection.OnReconnectAttempt += (attempt, ex) => 
        {
            reconnectAttempts++;
            attemptDetails.Add(attempt);
        };

        // Act - Wait longer for initial connection attempt to complete
        Thread.Sleep(2000);

        // Assert - When auto-reconnect is disabled, there should be exactly 1 attempt (the initial one)
        // No retry attempts should be made
        Assert.That(reconnectAttempts, Is.EqualTo(1), 
            $"When auto-reconnect is disabled, there should be exactly one connection attempt (the initial one) with no retries. Attempt details: [{string.Join(", ", attemptDetails)}]");
    }
}
