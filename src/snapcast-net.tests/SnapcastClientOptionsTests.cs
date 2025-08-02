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

namespace SnapCastNet.tests;

[TestFixture]
public class SnapcastClientOptionsTests
{
    [Test]
    public void Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var options = new SnapcastClientOptions();

        // Assert
        Assert.That(options.ConnectionTimeoutMs, Is.EqualTo(5000));
        Assert.That(options.MaxRetryAttempts, Is.EqualTo(3));
        Assert.That(options.HealthCheckIntervalMs, Is.EqualTo(30000));
        Assert.That(options.EnableAutoReconnect, Is.True);
        Assert.That(options.ReconnectDelayMs, Is.EqualTo(1000));
        Assert.That(options.MaxReconnectDelayMs, Is.EqualTo(30000));
        Assert.That(options.UseExponentialBackoff, Is.True);
        Assert.That(options.EnableVerboseLogging, Is.False);
        Assert.That(options.BufferSize, Is.EqualTo(1024));
    }

    [Test]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new SnapcastClientOptions();

        // Act
        options.ConnectionTimeoutMs = 10000;
        options.MaxRetryAttempts = 5;
        options.HealthCheckIntervalMs = 60000;
        options.EnableAutoReconnect = false;
        options.ReconnectDelayMs = 2000;
        options.MaxReconnectDelayMs = 60000;
        options.UseExponentialBackoff = false;
        options.EnableVerboseLogging = true;
        options.BufferSize = 2048;

        // Assert
        Assert.That(options.ConnectionTimeoutMs, Is.EqualTo(10000));
        Assert.That(options.MaxRetryAttempts, Is.EqualTo(5));
        Assert.That(options.HealthCheckIntervalMs, Is.EqualTo(60000));
        Assert.That(options.EnableAutoReconnect, Is.False);
        Assert.That(options.ReconnectDelayMs, Is.EqualTo(2000));
        Assert.That(options.MaxReconnectDelayMs, Is.EqualTo(60000));
        Assert.That(options.UseExponentialBackoff, Is.False);
        Assert.That(options.EnableVerboseLogging, Is.True);
        Assert.That(options.BufferSize, Is.EqualTo(2048));
    }
}
