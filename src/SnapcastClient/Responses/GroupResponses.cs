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
/// Response containing group status information
/// </summary>
public struct GroupStatus
{
    /// <summary>
    /// Gets or sets the group information
    /// </summary>
    [JsonProperty("group")]
    public Models.Group Group;
}

/// <summary>
/// Response for a group mute set operation
/// </summary>
public struct GroupMuteSet
{
    /// <summary>
    /// Gets or sets the mute state that was set
    /// </summary>
    [JsonProperty("mute")]
    public bool Mute;
}

/// <summary>
/// Response for a group stream set operation
/// </summary>
public struct GroupStreamSet
{
    /// <summary>
    /// Gets or sets the stream ID that was assigned to the group
    /// </summary>
    [JsonProperty("stream_id")]
    public string StreamId;
}

/// <summary>
/// Response for a group clients set operation
/// </summary>
public struct GroupClientsSet
{
    /// <summary>
    /// Gets or sets the updated server information with new group configuration
    /// </summary>
    [JsonProperty("server")]
    public Models.Server Server;
}

/// <summary>
/// Response for a group name set operation
/// </summary>
public struct GroupNameSet
{
    /// <summary>
    /// Gets or sets the name that was set for the group
    /// </summary>
    [JsonProperty("name")]
    public string Name;
}
