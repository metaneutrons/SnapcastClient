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

namespace SnapcastClient;

/// <summary>
/// Extension methods for configuring Snapcast services in dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Snapcast client services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="host">Snapcast server host</param>
    /// <param name="port">Snapcast server port</param>
    /// <param name="configure">Optional configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSnapcastClient(
        this IServiceCollection services,
        string host,
        int port,
        Action<SnapcastClientOptions>? configure = null
    )
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null, empty, or whitespace", nameof(host));

        if (port <= 0 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");

        // Configure options
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<SnapcastClientOptions>(_ => { });
        }

        // Register connection factory
        services.AddSingleton<Func<IConnection>>(serviceProvider =>
        {
            return () =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<SnapcastClientOptions>>().Value;
                var logger = serviceProvider.GetService<ILogger<ResilientTcpConnection>>();
                var timeProvider = serviceProvider.GetService<TimeProvider>() ?? TimeProvider.System;
                return new ResilientTcpConnection(host, port, options, logger, timeProvider);
            };
        });

        // Register client
        services.AddSingleton<IClient>(serviceProvider =>
        {
            var connectionFactory = serviceProvider.GetRequiredService<Func<IConnection>>();
            var logger = serviceProvider.GetService<ILogger<Client>>();
            return new Client(connectionFactory(), logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Snapcast client services with a simple TCP connection (no resilience features)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="host">Snapcast server host</param>
    /// <param name="port">Snapcast server port</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSnapcastClientSimple(this IServiceCollection services, string host, int port)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null, empty, or whitespace", nameof(host));

        if (port <= 0 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");

        // Register simple connection factory
        services.AddSingleton<Func<IConnection>>(serviceProvider => () => new TcpConnection(host, port));

        // Register client
        services.AddSingleton<IClient>(serviceProvider =>
        {
            var connectionFactory = serviceProvider.GetRequiredService<Func<IConnection>>();
            var logger = serviceProvider.GetService<ILogger<Client>>();
            return new Client(connectionFactory(), logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Snapcast client services with a custom connection factory
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionFactory">Factory function to create connections</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSnapcastClient(
        this IServiceCollection services,
        Func<IServiceProvider, IConnection> connectionFactory
    )
    {
        if (connectionFactory == null)
            throw new ArgumentNullException(nameof(connectionFactory));

        // Register connection factory
        services.AddSingleton<Func<IConnection>>(serviceProvider => () => connectionFactory(serviceProvider));

        // Register client
        services.AddSingleton<IClient>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<Func<IConnection>>();
            var logger = serviceProvider.GetService<ILogger<Client>>();
            return new Client(factory(), logger);
        });

        return services;
    }
}
