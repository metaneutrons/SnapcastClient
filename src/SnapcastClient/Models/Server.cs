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
/// Information about the Snapcast server software
/// </summary>
public struct SnapServerInfo
{
    /// <summary>
    /// Gets the protocol version supported by the server
    /// </summary>
    [JsonProperty("protocolVersion")]
    public int ProtocolVersion;

    /// <summary>
    /// Gets the control protocol version supported by the server
    /// </summary>
    [JsonProperty("controlProtocolVersion")]
    public int ControlProtocolVersion;

    /// <summary>
    /// Gets the name of the Snapcast server software
    /// </summary>
    [JsonProperty("name")]
    public string Name;

    /// <summary>
    /// Gets the version of the Snapcast server software
    /// </summary>
    [JsonProperty("version")]
    public string Version;
}

/// <summary>
/// Information about the server host and software
/// </summary>
public struct ServerInfo
{
    /// <summary>
    /// Gets the host information for the server
    /// </summary>
    [JsonProperty("host")]
    public Host Host;

    /// <summary>
    /// Gets the Snapcast server software information
    /// </summary>
    [JsonProperty("snapserver")]
    public SnapServerInfo SnapServer;
}

/// <summary>
/// Represents a complete Snapcast server with all its groups and streams
/// </summary>
public struct Server
{
    /// <summary>
    /// Gets the server information including host and software details
    /// </summary>
    [JsonProperty("server")]
    public ServerInfo ServerInfo;

    /// <summary>
    /// Gets the list of groups managed by the server
    /// </summary>
    [JsonProperty("groups")]
    public List<Group> Groups;

    /// <summary>
    /// Gets the list of streams available on the server
    /// </summary>
    [JsonProperty("streams")]
    public List<Stream> Streams;
}

/// <summary>
/// Version information for the JSON-RPC protocol
/// </summary>
public struct RpcVersion
{
    /// <summary>
    /// Gets the major version number
    /// </summary>
    [JsonProperty("major")]
    public int Major;

    /// <summary>
    /// Gets the minor version number
    /// </summary>
    [JsonProperty("minor")]
    public int Minor;

    /// <summary>
    /// Gets the patch version number
    /// </summary>
    [JsonProperty("patch")]
    public int Patch;
}
