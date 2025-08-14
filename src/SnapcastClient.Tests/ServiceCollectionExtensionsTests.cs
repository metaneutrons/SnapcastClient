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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq;

namespace SnapcastClient.tests;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private ServiceCollection _services;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
        _services.AddLogging();
    }

    [Test]
    public void AddSnapcastClient_WithValidParameters_RegistersServices()
    {
        // Arrange & Act
        _services.AddSnapcastClient("localhost", 1705);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert - Only check service registrations, don't instantiate client
        var connectionFactory = serviceProvider.GetService<Func<IConnection>>();
        var options = serviceProvider.GetService<IOptions<SnapcastClientOptions>>();

        Assert.That(connectionFactory, Is.Not.Null);
        Assert.That(options, Is.Not.Null);
        
        // Verify IClient is registered without instantiating it
        var clientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IClient));
        Assert.That(clientDescriptor, Is.Not.Null);
        Assert.That(clientDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }

    [Test]
    public void AddSnapcastClient_WithConfiguration_AppliesOptions()
    {
        // Arrange & Act
        _services.AddSnapcastClient("localhost", 1705, options =>
        {
            options.EnableAutoReconnect = false;
            options.MaxRetryAttempts = 10;
            options.HealthCheckIntervalMs = 60000;
        });
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<SnapcastClientOptions>>();
        Assert.That(options.Value.EnableAutoReconnect, Is.False);
        Assert.That(options.Value.MaxRetryAttempts, Is.EqualTo(10));
        Assert.That(options.Value.HealthCheckIntervalMs, Is.EqualTo(60000));
    }

    [Test]
    public void AddSnapcastClient_WithNullHost_ThrowsArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _services.AddSnapcastClient(null!, 1705));
    }

    [Test]
    public void AddSnapcastClient_WithEmptyHost_ThrowsArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _services.AddSnapcastClient("", 1705));
    }

    [Test]
    public void AddSnapcastClient_WithInvalidPort_ThrowsArgumentOutOfRangeException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            _services.AddSnapcastClient("localhost", 0));
        
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            _services.AddSnapcastClient("localhost", 65536));
        
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            _services.AddSnapcastClient("localhost", -1));
    }

    [Test]
    public void AddSnapcastClient_WithCustomFactory_RegistersServices()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        Func<IServiceProvider, IConnection> factory = _ => mockConnection.Object;

        // Act
        _services.AddSnapcastClient(factory);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IClient>();
        var connectionFactory = serviceProvider.GetService<Func<IConnection>>();

        Assert.That(client, Is.Not.Null);
        Assert.That(connectionFactory, Is.Not.Null);
    }

    [Test]
    public void AddSnapcastClient_WithNullFactory_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _services.AddSnapcastClient((Func<IServiceProvider, IConnection>)null!));
    }

    [Test]
    public void AddSnapcastClient_RegistersAsSingleton()
    {
        // Arrange
        _services.AddSnapcastClient("localhost", 1705);

        // Assert - Check service registration without instantiating
        var clientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IClient));
        Assert.That(clientDescriptor, Is.Not.Null);
        Assert.That(clientDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }

    [Test]
    public void AddSnapcastClient_WithLogging_InjectsLogger()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Client>>();
        _services.AddSingleton(mockLogger.Object);
        _services.AddSnapcastClient("localhost", 1705);

        // Assert - Verify services are registered correctly
        var serviceProvider = _services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<Client>>();
        var connectionFactory = serviceProvider.GetService<Func<IConnection>>();

        Assert.That(logger, Is.Not.Null);
        Assert.That(connectionFactory, Is.Not.Null);
        
        // Verify IClient is registered
        var clientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IClient));
        Assert.That(clientDescriptor, Is.Not.Null);
    }

    [Test]
    public void AddSnapcastClient_ValidPortRange_DoesNotThrow()
    {
        // Arrange & Act & Assert
        Assert.DoesNotThrow(() => _services.AddSnapcastClient("localhost", 1));
        Assert.DoesNotThrow(() => _services.AddSnapcastClient("localhost", 1705));
        Assert.DoesNotThrow(() => _services.AddSnapcastClient("localhost", 65535));
    }

    [Test]
    public void AddSnapcastClient_ReturnsServiceCollection_ForChaining()
    {
        // Arrange & Act
        var result = _services.AddSnapcastClient("localhost", 1705);

        // Assert
        Assert.That(result, Is.SameAs(_services));
    }
}
