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
public class ClientLoggingTests
{
    private Mock<IConnection> _mockConnection;
    private Mock<ILogger<Client>> _mockLogger;

    [SetUp]
    public void SetUp()
    {
        _mockConnection = new Mock<IConnection>();
        _mockLogger = new Mock<ILogger<Client>>();
    }

    [Test]
    public void Constructor_WithLogger_DoesNotThrow()
    {
        // Arrange & Act & Assert
        Assert.DoesNotThrow(() =>
        {
            using var client = new Client(_mockConnection.Object, _mockLogger.Object);
        });
    }

    [Test]
    public void Constructor_WithoutLogger_DoesNotThrow()
    {
        // Arrange & Act & Assert
        Assert.DoesNotThrow(() =>
        {
            using var client = new Client(_mockConnection.Object, null);
        });
    }

    [Test]
    public void Constructor_WithNullConnection_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Client(null!, _mockLogger.Object));
    }

    [Test]
    public void Dispose_WhenCalled_DoesNotThrow()
    {
        // Arrange
        var client = new Client(_mockConnection.Object, _mockLogger.Object);

        // Act & Assert
        Assert.DoesNotThrow(() => client.Dispose());
    }

    [Test]
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var client = new Client(_mockConnection.Object, _mockLogger.Object);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            client.Dispose();
            client.Dispose(); // Second call should not throw
        });
    }

    [Test]
    public void ClientGetStatusAsync_WhenDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var client = new Client(_mockConnection.Object, _mockLogger.Object);
        client.Dispose();

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await client.ClientGetStatusAsync("test-client"));
    }

    [Test]
    public void ClientGetStatusAsync_WithNullId_ThrowsArgumentException()
    {
        // Arrange
        using var client = new Client(_mockConnection.Object, _mockLogger.Object);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await client.ClientGetStatusAsync(null!));
    }

    [Test]
    public void ClientGetStatusAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        using var client = new Client(_mockConnection.Object, _mockLogger.Object);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await client.ClientGetStatusAsync(""));
    }

    [Test]
    public void Constructor_LogsInitialization()
    {
        // Arrange & Act
        using var client = new Client(_mockConnection.Object, _mockLogger.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Initializing Snapcast client")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void Dispose_LogsDisposal()
    {
        // Arrange
        var client = new Client(_mockConnection.Object, _mockLogger.Object);

        // Act
        client.Dispose();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Disposing Snapcast client")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void Constructor_WithDisposableConnection_DisposesConnectionOnDispose()
    {
        // Arrange
        var mockDisposableConnection = new Mock<IConnection>();
        mockDisposableConnection.As<IDisposable>();
        var client = new Client(mockDisposableConnection.Object, _mockLogger.Object);

        // Act
        client.Dispose();

        // Assert
        mockDisposableConnection.As<IDisposable>().Verify(x => x.Dispose(), Times.Once);
    }

    [Test]
    public void Constructor_WithNonDisposableConnection_DoesNotThrowOnDispose()
    {
        // Arrange
        var client = new Client(_mockConnection.Object, _mockLogger.Object);

        // Act & Assert
        Assert.DoesNotThrow(() => client.Dispose());
    }
}
