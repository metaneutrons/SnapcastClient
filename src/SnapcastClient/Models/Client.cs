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

using Newtonsoft.Json;

namespace SnapcastClient.Models;

/// <summary>
/// Configuration settings for a Snapcast client
/// </summary>
public struct ClientConfig
{
    /// <summary>
    /// Gets the instance number of the client
    /// </summary>
    [JsonProperty("instance")]
    public required int Instance { get; init; }

    /// <summary>
    /// Gets the latency value in milliseconds for audio synchronization
    /// </summary>
    [JsonProperty("latency")]
    public required int Latency { get; init; }

    /// <summary>
    /// Gets the display name of the client
    /// </summary>
    [JsonProperty("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the volume configuration of the client
    /// </summary>
    [JsonProperty("volume")]
    public required ClientVolume Volume { get; init; }
}

/// <summary>
/// Host information for a Snapcast client
/// </summary>
public struct ClientHost
{
    /// <summary>
    /// Gets the system architecture of the client host
    /// </summary>
    [JsonProperty("arch")]
    public required string Arch { get; init; }

    /// <summary>
    /// Gets the IP address of the client host
    /// </summary>
    [JsonProperty("ip")]
    public required string Ip { get; init; }

    /// <summary>
    /// Gets the MAC address of the client host
    /// </summary>
    [JsonProperty("mac")]
    public required string Mac { get; init; }

    /// <summary>
    /// Gets the hostname of the client
    /// </summary>
    [JsonProperty("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the operating system of the client host
    /// </summary>
    [JsonProperty("os")]
    public required string Os { get; init; }
}

/// <summary>
/// Volume configuration for a Snapcast client
/// </summary>
public struct ClientVolume
{
    /// <summary>
    /// Gets a value indicating whether the client is muted
    /// </summary>
    [JsonProperty("muted")]
    public required bool Muted { get; init; }

    /// <summary>
    /// Gets the volume percentage (0-100)
    /// </summary>
    [JsonProperty("percent")]
    public required int Percent { get; init; }
}

/// <summary>
/// Timestamp information for when a client was last seen
/// </summary>
public struct LastSeen
{
    /// <summary>
    /// Gets the seconds component of the timestamp
    /// </summary>
    [JsonProperty("sec")]
    public required long Sec { get; init; }

    /// <summary>
    /// Gets the microseconds component of the timestamp
    /// </summary>
    [JsonProperty("usec")]
    public required int Usec { get; init; }
}

/// <summary>
/// Information about the Snapcast client software
/// </summary>
public struct SnapClientInfo
{
    /// <summary>
    /// Gets the name of the Snapcast client software
    /// </summary>
    [JsonProperty("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the protocol version supported by the client
    /// </summary>
    [JsonProperty("protocolVersion")]
    public required int ProtocolVersion { get; init; }

    /// <summary>
    /// Gets the version of the Snapcast client software
    /// </summary>
    [JsonProperty("version")]
    public required string Version { get; init; }
}

/// <summary>
/// Represents a complete Snapcast client with all its properties
/// </summary>
public struct SnapClient
{
    /// <summary>
    /// Gets the configuration settings for the client
    /// </summary>
    [JsonProperty("config")]
    public required ClientConfig Config { get; init; }

    /// <summary>
    /// Gets a value indicating whether the client is currently connected
    /// </summary>
    [JsonProperty("connected")]
    public required bool Connected { get; init; }

    /// <summary>
    /// Gets the host information for the client
    /// </summary>
    [JsonProperty("host")]
    public required ClientHost Host { get; init; }

    /// <summary>
    /// Gets the unique identifier of the client
    /// </summary>
    [JsonProperty("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the timestamp when the client was last seen
    /// </summary>
    [JsonProperty("lastSeen")]
    public required LastSeen LastSeen { get; init; }

    /// <summary>
    /// Gets the Snapcast client software information
    /// </summary>
    [JsonProperty("snapclient")]
    public required SnapClientInfo ClientInfo { get; init; }
}
