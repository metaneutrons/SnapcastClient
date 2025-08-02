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
public class ConnectionStateTests
{
    [Test]
    public void ConnectionState_HasExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<ConnectionState>();

        // Assert
        Assert.That(values, Contains.Item(ConnectionState.Disconnected));
        Assert.That(values, Contains.Item(ConnectionState.Connecting));
        Assert.That(values, Contains.Item(ConnectionState.Connected));
        Assert.That(values, Contains.Item(ConnectionState.Degraded));
        Assert.That(values, Contains.Item(ConnectionState.Reconnecting));
        Assert.That(values, Contains.Item(ConnectionState.Failed));
    }

    [Test]
    public void ConnectionState_HasCorrectCount()
    {
        // Arrange & Act
        var values = Enum.GetValues<ConnectionState>();

        // Assert
        Assert.That(values.Length, Is.EqualTo(6));
    }

    [Test]
    public void ConnectionState_CanBeConvertedToString()
    {
        // Arrange & Act & Assert
        Assert.That(ConnectionState.Disconnected.ToString(), Is.EqualTo("Disconnected"));
        Assert.That(ConnectionState.Connecting.ToString(), Is.EqualTo("Connecting"));
        Assert.That(ConnectionState.Connected.ToString(), Is.EqualTo("Connected"));
        Assert.That(ConnectionState.Degraded.ToString(), Is.EqualTo("Degraded"));
        Assert.That(ConnectionState.Reconnecting.ToString(), Is.EqualTo("Reconnecting"));
        Assert.That(ConnectionState.Failed.ToString(), Is.EqualTo("Failed"));
    }

    [Test]
    public void ConnectionState_CanBeCompared()
    {
        // Arrange & Act & Assert
        Assert.That(ConnectionState.Disconnected, Is.EqualTo(ConnectionState.Disconnected));
        Assert.That(ConnectionState.Connected, Is.Not.EqualTo(ConnectionState.Disconnected));
        Assert.That(ConnectionState.Connecting, Is.Not.EqualTo(ConnectionState.Failed));
    }

    [Test]
    public void ConnectionState_CanBeUsedInSwitch()
    {
        // Arrange
        var state = ConnectionState.Connected;
        var result = "";

        // Act
        switch (state)
        {
            case ConnectionState.Disconnected:
                result = "disconnected";
                break;
            case ConnectionState.Connecting:
                result = "connecting";
                break;
            case ConnectionState.Connected:
                result = "connected";
                break;
            case ConnectionState.Degraded:
                result = "degraded";
                break;
            case ConnectionState.Reconnecting:
                result = "reconnecting";
                break;
            case ConnectionState.Failed:
                result = "failed";
                break;
        }

        // Assert
        Assert.That(result, Is.EqualTo("connected"));
    }
}
