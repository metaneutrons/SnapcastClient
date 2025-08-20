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
/// Tests for Phase 3 circuit breaker pattern implementation.
/// Demonstrates enterprise-grade resilience and fault tolerance capabilities.
/// </summary>
[TestFixture]
public class CircuitBreakerTests
{
    private Mock<ILogger> _mockLogger;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger>();
    }

    [Test]
    public void CircuitBreaker_ShouldStartInClosedState()
    {
        // Arrange & Act
        var circuitBreaker = new CircuitBreaker();

        // Assert
        Assert.That(circuitBreaker.State, Is.EqualTo(CircuitBreakerState.Closed));
    }

    [Test]
    public async Task CircuitBreaker_ShouldAllowOperationWhenClosed()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker();
        var operationExecuted = false;

        // Act
        await circuitBreaker.ExecuteAsync(async () =>
        {
            operationExecuted = true;
            await Task.CompletedTask;
        });

        // Assert
        Assert.That(operationExecuted, Is.True);
        Assert.That(circuitBreaker.State, Is.EqualTo(CircuitBreakerState.Closed));
    }

    [Test]
    public async Task CircuitBreaker_ShouldOpenAfterFailureThreshold()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 3,
            Timeout = TimeSpan.FromSeconds(1),
        };
        var circuitBreaker = new CircuitBreaker(options, _mockLogger.Object);

        // Act - Cause failures to exceed threshold
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await circuitBreaker.ExecuteAsync<int>(async () =>
                {
                    await Task.CompletedTask;
                    throw new InvalidOperationException("Test failure");
                });
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Assert
        Assert.That(circuitBreaker.State, Is.EqualTo(CircuitBreakerState.Open));
    }

    [Test]
    public async Task CircuitBreaker_ShouldBlockOperationsWhenOpen()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            Timeout = TimeSpan.FromSeconds(1),
        };
        var circuitBreaker = new CircuitBreaker(options, _mockLogger.Object);

        // Cause circuit breaker to open
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await circuitBreaker.ExecuteAsync<int>(async () =>
                {
                    await Task.CompletedTask;
                    throw new InvalidOperationException("Test failure");
                });
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        // Act & Assert
        Assert.ThrowsAsync<CircuitBreakerOpenException>(async () =>
        {
            await circuitBreaker.ExecuteAsync(async () =>
            {
                await Task.CompletedTask;
            });
        });

        Assert.That(circuitBreaker.State, Is.EqualTo(CircuitBreakerState.Open));
    }

    [Test]
    public async Task CircuitBreaker_ShouldTransitionToHalfOpenAfterTimeout()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            Timeout = TimeSpan.FromMilliseconds(100), // Short timeout for testing
        };
        var circuitBreaker = new CircuitBreaker(options, _mockLogger.Object);

        // Cause circuit breaker to open
        for (int i = 0; i < 2; i++)
        {
            try
            {
                await circuitBreaker.ExecuteAsync<int>(async () =>
                {
                    await Task.CompletedTask;
                    throw new InvalidOperationException("Test failure");
                });
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        Assert.That(circuitBreaker.State, Is.EqualTo(CircuitBreakerState.Open));

        // Act - Wait for timeout and try operation
        await Task.Delay(150); // Wait longer than timeout

        var operationExecuted = false;
        // Need multiple successful operations to transition from HalfOpen to Closed (default SuccessThreshold = 3)
        for (int i = 0; i < 3; i++)
        {
            await circuitBreaker.ExecuteAsync(async () =>
            {
                operationExecuted = true;
                await Task.CompletedTask;
            });
        }

        // Assert
        Assert.That(operationExecuted, Is.True);
        Assert.That(circuitBreaker.State, Is.EqualTo(CircuitBreakerState.Closed)); // Should close after successful operations
    }

    [Test]
    public void CircuitBreaker_ShouldResetToClosedState()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 1 };
        var circuitBreaker = new CircuitBreaker(options, _mockLogger.Object);

        // Cause failure to open circuit
        try
        {
            circuitBreaker
                .ExecuteAsync<int>(async () =>
                {
                    await Task.CompletedTask;
                    throw new InvalidOperationException("Test failure");
                })
                .Wait();
        }
        catch
        {
            // Expected
        }

        Assert.That(circuitBreaker.State, Is.EqualTo(CircuitBreakerState.Open));

        // Act
        circuitBreaker.Reset();

        // Assert
        Assert.That(circuitBreaker.State, Is.EqualTo(CircuitBreakerState.Closed));
        var stats = circuitBreaker.Stats;
        Assert.That(stats.FailureCount, Is.EqualTo(0));
        Assert.That(stats.SuccessCount, Is.EqualTo(0));
    }

    [Test]
    public void CircuitBreakerStats_ShouldProvideAccurateInformation()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker();

        // Act
        var stats = circuitBreaker.Stats;

        // Assert
        Assert.That(stats.State, Is.EqualTo(CircuitBreakerState.Closed));
        Assert.That(stats.FailureCount, Is.EqualTo(0));
        Assert.That(stats.SuccessCount, Is.EqualTo(0));
        Assert.That(stats.LastFailureTime, Is.EqualTo(DateTime.MinValue));
        Assert.That(stats.ToString(), Does.Contain("State: Closed"));
    }

    [Test]
    public void CircuitBreakerOptions_ShouldHaveReasonableDefaults()
    {
        // Arrange & Act
        var options = new CircuitBreakerOptions();

        // Assert
        Assert.That(options.FailureThreshold, Is.EqualTo(5));
        Assert.That(options.Timeout, Is.EqualTo(TimeSpan.FromSeconds(60)));
        Assert.That(options.SuccessThreshold, Is.EqualTo(3));
    }

    [Test]
    public void ConnectionHealthMonitor_ShouldStartHealthy()
    {
        // Arrange & Act
        using var monitor = new ConnectionHealthMonitor(_mockLogger.Object);

        // Assert
        Assert.That(monitor.IsHealthy(), Is.True);
    }

    [Test]
    public void ConnectionHealthMonitor_ShouldRecordMessages()
    {
        // Arrange
        using var monitor = new ConnectionHealthMonitor(_mockLogger.Object);

        // Act
        monitor.RecordMessageReceived();
        var stats = monitor.GetHealthStats();

        // Assert
        Assert.That(stats.TotalMessages, Is.EqualTo(1));
        Assert.That(stats.IsHealthy, Is.True);
    }

    [Test]
    public void ConnectionHealthStats_ShouldCalculateTotalChecksCorrectly()
    {
        // Arrange
        var stats = new ConnectionHealthStats { HealthyChecks = 8, UnhealthyChecks = 2 };

        // Act & Assert
        Assert.That(stats.TotalChecks, Is.EqualTo(10));
    }

    [Test]
    public void ConnectionDiagnostics_ShouldCombineAllStats()
    {
        // Arrange
        var diagnostics = new ConnectionDiagnostics
        {
            ProcessingStats = new MessageProcessingStats { IsHealthy = true },
            CircuitBreakerStats = new CircuitBreakerStats { State = CircuitBreakerState.Closed },
            HealthStats = new ConnectionHealthStats { IsHealthy = true },
            OverallHealth = true,
        };

        // Act
        var summary = diagnostics.ToString();

        // Assert
        Assert.That(summary, Does.Contain("Overall Health: Healthy"));
        Assert.That(summary, Does.Contain("Circuit Breaker"));
        Assert.That(summary, Does.Contain("Health Monitor"));
    }

    [Test]
    public void IConnection_ShouldHaveCircuitBreakerMethods()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();

        // Act & Assert - Verify Phase 3 methods are available in interface
        Assert.That(typeof(IConnection).GetMethod("GetCircuitBreakerStats"), Is.Not.Null);
        Assert.That(typeof(IConnection).GetMethod("GetCircuitBreakerState"), Is.Not.Null);
        Assert.That(typeof(IConnection).GetMethod("ResetCircuitBreaker"), Is.Not.Null);
        Assert.That(typeof(IConnection).GetMethod("GetDiagnostics"), Is.Not.Null);
        Assert.That(typeof(IConnection).GetEvent("CircuitBreakerStateChanged"), Is.Not.Null);
    }

    [Test]
    public async Task CircuitBreaker_ShouldFireStateChangeEvents()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 1 };
        var circuitBreaker = new CircuitBreaker(options, _mockLogger.Object);

        CircuitBreakerState? oldState = null;
        CircuitBreakerState? newState = null;

        circuitBreaker.StateChanged += (old, @new) =>
        {
            oldState = old;
            newState = @new;
        };

        // Act - Cause failure to trigger state change
        try
        {
            await circuitBreaker.ExecuteAsync<int>(async () =>
            {
                await Task.CompletedTask;
                throw new InvalidOperationException("Test failure");
            });
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert
        Assert.That(oldState, Is.EqualTo(CircuitBreakerState.Closed));
        Assert.That(newState, Is.EqualTo(CircuitBreakerState.Open));
    }

    [Test]
    public void CircuitBreakerOpenException_ShouldHaveCorrectMessage()
    {
        // Arrange & Act
        var exception = new CircuitBreakerOpenException("Test message");

        // Assert
        Assert.That(exception.Message, Is.EqualTo("Test message"));
        Assert.That(exception, Is.InstanceOf<Exception>());
    }

    [Test]
    public void CircuitBreakerOpenException_ShouldSupportInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner");

        // Act
        var exception = new CircuitBreakerOpenException("Test message", innerException);

        // Assert
        Assert.That(exception.Message, Is.EqualTo("Test message"));
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }
}
