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

namespace SnapcastClient.Params;

/// <summary>
/// Parameters containing a client identifier
/// </summary>
public struct ClientId
{
    /// <summary>
    /// Gets or sets the unique identifier of the client
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}

/// <summary>
/// Parameters for getting the status of a specific client
/// </summary>
public struct ClientGetStatus
{
    /// <summary>
    /// Gets or sets the unique identifier of the client
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}

/// <summary>
/// Volume configuration parameters for a client
/// </summary>
public struct ClientVolume
{
    /// <summary>
    /// Gets or sets a value indicating whether the client is muted
    /// </summary>
    [JsonProperty("muted")]
    public bool Muted;

    /// <summary>
    /// Gets or sets the volume percentage (0-100)
    /// </summary>
    [JsonProperty("percent")]
    public int Percent;
}

/// <summary>
/// Parameters for setting the volume of a specific client
/// </summary>
public struct ClientSetVolume
{
    /// <summary>
    /// Gets or sets the unique identifier of the client
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the volume configuration to apply
    /// </summary>
    [JsonProperty("volume")]
    public ClientVolume Volume { get; set; }
}

/// <summary>
/// Parameters for setting the latency of a specific client
/// </summary>
public struct ClientSetLatency
{
    /// <summary>
    /// Gets or sets the unique identifier of the client
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the latency value in milliseconds
    /// </summary>
    [JsonProperty("latency")]
    public int Latency { get; set; }
}

/// <summary>
/// Parameters for setting the name of a specific client
/// </summary>
public struct ClientSetName
{
    /// <summary>
    /// Gets or sets the unique identifier of the client
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the new name for the client
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}
