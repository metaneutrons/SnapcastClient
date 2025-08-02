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

namespace SnapcastClient.Responses;

/// <summary>
/// Response containing client status information
/// </summary>
public struct ClientStatus
{
    /// <summary>
    /// Gets or sets the client information
    /// </summary>
    [JsonProperty("client")]
    public Models.SnapClient Client;
}

/// <summary>
/// Response for a volume set operation
/// </summary>
public struct VolumeSet
{
    /// <summary>
    /// Gets or sets the volume that was set
    /// </summary>
    [JsonProperty("volume")]
    public Models.ClientVolume Volume;
}

/// <summary>
/// Response for a latency set operation
/// </summary>
public struct LatencySet
{
    /// <summary>
    /// Gets or sets the latency that was set
    /// </summary>
    [JsonProperty("latency")]
    public int Latency;
}

/// <summary>
/// Response for a name set operation
/// </summary>
public struct NameSet
{
    /// <summary>
    /// Gets or sets the name that was set
    /// </summary>
    [JsonProperty("name")]
    public string Name;
}
